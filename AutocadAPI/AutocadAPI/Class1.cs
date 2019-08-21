using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadAPI
{
    public class Class1
    {
        [CommandMethod("AttachXref")]
        public void AttachXref()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            string fileName = @"D:\Coding Trials\GIT\AutocadAPI\AutocadAPI\AutocadAPI\Files\hamada.dwg";
            string strBlkName = System.IO.Path.GetFileNameWithoutExtension(fileName);

            ObjectId objId = acCurDb.AttachXref(fileName, strBlkName);
        }

        [CommandMethod("ListEntities")]
        public void ListEntities()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable blkTbl = trans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord blkTblRec = trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                string msg = "\nModel Space Obects: ";
                int count = 0;
                foreach (ObjectId objId in blkTblRec)
                {
                    msg += "\n" + objId.ObjectClass.DxfName;
                    count += 1;
                }

                if(count == 0)
                {
                    msg = "no objects in the model space: ";
                }

                acDoc.Editor.WriteMessage(msg);
            }
        }

        [CommandMethod("AddLayer")]
        public void AddLayer()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lyrTbl = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (!(lyrTbl.Has("hamadaLayer")))
                {
                    trans.GetObject(db.LayerTableId, OpenMode.ForWrite);

                    using (LayerTableRecord lyrTblRec = new LayerTableRecord())
                    {
                        lyrTblRec.Name = "HamadaLayer";
                        lyrTblRec.Color = Color.FromColor(System.Drawing.Color.Cyan);
                         
                        lyrTbl.Add(lyrTblRec);

                        trans.AddNewlyCreatedDBObject(lyrTblRec, true);

                        LayerTableRecord lyrZeroRec = trans.GetObject(lyrTbl["0"], OpenMode.ForWrite) as LayerTableRecord;
                        lyrZeroRec.Color = Color.FromColor(System.Drawing.Color.Red);
                    }
                    
                    
                    trans.Commit();
                }
            }
        }
    }
}
