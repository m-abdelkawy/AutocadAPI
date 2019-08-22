using AutocadAPI.BuildingElements;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutocadAPI
{
    public class Class1
    {
        private List<List<Point3d>> lstMidPoints = new List<List<Point3d>>();
        List<double> lstThickness = new List<double>();
        List<Wall> lstWalls = new List<Wall>();

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

                if (count == 0)
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

        [CommandMethod("trialwallPts")]
        public void TrialWallPts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            List<Line> lstWallLines = new List<Line>();

            ObjectId lyrWallId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //wall layer
                LayerTable lyrTbl = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (!lyrTbl.Has("Wall"))
                {
                    return;
                }
                lyrWallId = lyrTbl["Wall"];
                //LayerTableRecord lyrTblRecWall = trans.GetObject(lyrWallId, OpenMode.ForRead) as LayerTableRecord;



                Editor ed = doc.Editor;
                PromptSelectionResult prSelRes = ed.GetSelection();
                if (prSelRes.Status == PromptStatus.OK)
                {
                    SelectionSet selSet = prSelRes.Value;
                    IEnumerator itr = selSet.GetEnumerator();

                    while (itr.MoveNext())
                    {
                        SelectedObject lineObj = itr.Current as SelectedObject;

                        if (lineObj != null)
                        {
                            Entity lineEnt = trans.GetObject(lineObj.ObjectId, OpenMode.ForRead) as Entity;
                            Line line = lineEnt as Line;

                        }
                    }
                }


            }
        }

        [CommandMethod("WallGetEndPoints")]
        public void WallGetEndPoints()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            List<List<Point3d>> lstMidPoints = new List<List<Point3d>>();
            List<double> lstThickness = new List<double>();


            List<Line> lstWallLines = new List<Line>();

            ObjectId lyrWallId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //wall layer
                LayerTable lyrTbl = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (!lyrTbl.Has("Wall"))
                {
                    return;
                }
                lyrWallId = lyrTbl["Wall"];


                BlockTable blkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blkTblModelSpaceRec = trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId objId in blkTblModelSpaceRec)
                {
                    Entity lineEnt = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                    Line line = lineEnt as Line;
                    if (line.LayerId == lyrWallId)
                    {
                        lstWallLines.Add(line);
                    }
                }

                List<List<Line>> lstWalls = new List<List<Line>>();


                for (int i = 0; i < lstWallLines.Count; i++)
                {
                    Line parallel = lstWallLines[i].LineGetNearestParallel(lstWallLines.ToArray());
                    if (parallel != null && (lstWallLines[i].Length > parallel.Length))
                    {
                        lstWalls.Add(new List<Line> { lstWallLines[i], parallel });
                        lstThickness.Add(MathHelper.DistanceBetweenTwoParallels(lstWallLines[i], parallel));
                    }
                }


                foreach (List<Line> lstParallels in lstWalls)
                {
                    Point3d stPt1 = lstParallels[0].StartPoint;
                    Point3d stPt2 = lstParallels[1].StartPoint;

                    Point3d endPt1 = lstParallels[0].EndPoint;
                    Point3d endPt2 = lstParallels[1].EndPoint;

                    if (stPt1.DistanceTo(stPt2) < stPt1.DistanceTo(endPt2))
                    {
                        lstMidPoints.Add(new List<Point3d> { MathHelper.MidPoint(stPt1, stPt2), MathHelper.MidPoint(endPt1, endPt2) });
                    }
                    else
                    {
                        lstMidPoints.Add(new List<Point3d> { MathHelper.MidPoint(stPt1, endPt2), MathHelper.MidPoint(stPt2, endPt1) });
                    }
                }
                trans.Commit();
            }
            this.lstMidPoints = lstMidPoints;
            this.lstThickness = lstThickness;

            for (int i = 0; i < lstMidPoints.Count; i++)
            {
                this.lstWalls.Add(new Wall(lstThickness[i], lstMidPoints[i][0], lstMidPoints[i][1]));
            }
        }
    }
}
