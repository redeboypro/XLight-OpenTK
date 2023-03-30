using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Assimp;
using Assimp.Configs;
using OpenTK;
using OpenTK.Graphics;
using XLight_OpenTK.Rendering;
using Mesh = XLight_OpenTK.Rendering.Mesh;

// ReSharper disable PossibleNullReferenceException

namespace XLight_OpenTK
{
    public static class LightX
    {
        private const int FaceVertexCount = 3;

        public static Color4 LightColor { get; set; } = new Color4(1.0f, 0.95f, 0.75f, 1.0f);

        public static float Ambient { get; set; } = 0.3f;

        public static float MinimumDiffuse { get; set; } = 0.35f;

        public static (Mesh, Texture) Bake(Face[] faces, Vector3 lightDirection, int lightmapSize, int numberOfRows)
        {
            var numberOfFaces = faces.Length;

            var roundedNumOfFacesInOneMap = (int) Math.Truncate((float) numberOfFaces / numberOfRows);
            var numberOfAdditionalFaces = numberOfFaces % numberOfRows;

            var numberOfFacesInCurrentRow = roundedNumOfFacesInOneMap;

            var numberOfColumns = roundedNumOfFacesInOneMap + numberOfAdditionalFaces;
            var lightmapTexture = new Bitmap(lightmapSize * numberOfColumns, lightmapSize * numberOfRows);

            var last = numberOfRows - 1;

            for (var row = 0; row < numberOfRows; row++)
            {
                if (row == last)
                {
                    numberOfFacesInCurrentRow = numberOfColumns;
                }

                var lightCache = new LightCache(lightmapTexture);

                var currentIndex = row * roundedNumOfFacesInOneMap;
                var length = currentIndex + numberOfFacesInCurrentRow;

                var localIndex = 0;
                for (var receiverIndex = currentIndex; receiverIndex < length; receiverIndex++)
                {
                    for (var projectorIndex = 0; projectorIndex < numberOfFaces; projectorIndex++)
                    {
                        if (projectorIndex == receiverIndex)
                        {
                            continue;
                        }

                        var projector = faces[projectorIndex];
                        var receiver = faces[receiverIndex];

                        var projectedPoints = new List<Vector3>();
                        var isIntersects = false;

                        for (var pointIndex = 0; pointIndex < FaceVertexCount; pointIndex++)
                        {
                            var point = projector[pointIndex].WorldLocation;
                            var receiverPoint = receiver[pointIndex];

                            if (RayIsIntersectsPlane(receiverPoint.WorldLocation, receiverPoint.Normal, point,
                                lightDirection,
                                out var hitPoint))
                            {
                                projectedPoints.Add(hitPoint);
                                isIntersects = true;
                            }
                            else
                            {
                                isIntersects = false;
                                break;
                            }
                        }

                        BakeFace(IsHigher(receiver.A.Normal),
                            lightmapSize,
                            projectedPoints,
                            localIndex, row,
                            numberOfFacesInCurrentRow, numberOfRows,
                            lightDirection,
                            !isIntersects,
                            ref receiver,
                            ref lightmapTexture,
                            ref lightCache);
                    }

                    localIndex++;
                }
            }

            var meshVertices = new List<Vector3>();
            var meshBaseUVs = new List<Vector2>();
            var meshLightmapUVs = new List<Vector2>();
            var meshIndices = new List<int>();

            var index = 0;
            foreach (var face in faces)
            {
                for (var v = 0; v < FaceVertexCount; v++)
                {
                    var vertex = face[v];
                    meshVertices.Add(vertex.WorldLocation);
                    meshBaseUVs.Add(vertex.BaseUV);
                    meshLightmapUVs.Add(vertex.LightmapUV);
                    meshIndices.Add(index + v);
                }

                index += FaceVertexCount;
            }

            var mesh = new Mesh(
                faces,
                meshVertices.ToArray(),
                meshBaseUVs.ToArray(),
                meshLightmapUVs.ToArray(),
                meshIndices.ToArray());

            return (mesh, new Texture(lightmapTexture));
        }

        private static void BakeFace(Axis axis,
            int lightmapUnitSize,
            IReadOnlyList<Vector3> projectedPoints,
            int faceIndex, int row,
            int numberOfFaces, int numberOfRows,
            Vector3 lightDirection,
            bool isOnlyDiffuseLighting,
            ref Face receiver, ref Bitmap lightmapTexture, ref LightCache lightCache)
        {
            int axisAIndex;
            int axisBIndex;

            //Projection plane
            switch (axis)
            {
                case Axis.X:
                    axisAIndex = 2;
                    axisBIndex = 1;
                    break;
                case Axis.Y:
                    axisAIndex = 0;
                    axisBIndex = 2;
                    break;
                case Axis.Z:
                    axisAIndex = 0;
                    axisBIndex = 1;
                    break;
                default:
                    throw new Exception("Unknown axis");
            }

            var receiverTextureCoordinates = new[]
            {
                new Vector2(receiver[0].WorldLocation[axisBIndex], receiver[0].WorldLocation[axisAIndex]),
                new Vector2(receiver[1].WorldLocation[axisBIndex], receiver[1].WorldLocation[axisAIndex]),
                new Vector2(receiver[2].WorldLocation[axisBIndex], receiver[2].WorldLocation[axisAIndex])
            };

            var projectorToPlane = new Vector2[]
            {
                //Empty projection
            };

            if (!isOnlyDiffuseLighting)
            {
                projectorToPlane = new[]
                {
                    new Vector2(projectedPoints[0][axisBIndex], projectedPoints[0][axisAIndex]),
                    new Vector2(projectedPoints[1][axisBIndex], projectedPoints[1][axisAIndex]),
                    new Vector2(projectedPoints[2][axisBIndex], projectedPoints[2][axisAIndex])
                };
            }

            var min = GetMin(receiverTextureCoordinates);

            for (var i = 0; i < FaceVertexCount; i++)
            {
                receiverTextureCoordinates[i] -= min;

                if (!isOnlyDiffuseLighting)
                {
                    projectorToPlane[i] -= min;
                }
            }

            var max = GetMax(receiverTextureCoordinates);

            var shiftX = (float) faceIndex / numberOfFaces;
            var shiftY = (float) row / numberOfRows;

            var scissorX = max.X * numberOfFaces;
            var scissorY = max.Y * numberOfRows;

            var textureHeight = lightmapTexture.Height;
            var textureWidth = lightmapTexture.Width;


            for (var i = 0; i < FaceVertexCount; i++)
            {
                //Receiver to uv conversion
                {
                    receiverTextureCoordinates[i].X /= scissorX;
                    receiverTextureCoordinates[i].X += shiftX;

                    receiverTextureCoordinates[i].Y /= scissorY;
                    receiverTextureCoordinates[i].Y += shiftY;
                }

                if (isOnlyDiffuseLighting)
                {
                    continue;
                }

                //Projector to uv conversion
                {
                    projectorToPlane[i].X /= scissorX;
                    projectorToPlane[i].X += shiftX;

                    projectorToPlane[i].Y /= scissorY;
                    projectorToPlane[i].Y += shiftY;
                }
            }


            receiver.A.LightmapUV = receiverTextureCoordinates[0];

            receiver.B.LightmapUV = receiverTextureCoordinates[1];

            receiver.C.LightmapUV = receiverTextureCoordinates[2];


            var diffuseFactor = Math.Max(Vector3.Dot(receiver.A.Normal, -lightDirection),
                MinimumDiffuse) + Ambient;
            var diffuseColor = ShadePixel(Math.Min(diffuseFactor, 1.0f));

            var shadowFactor = MinimumDiffuse + Ambient;
            var shadowColor = ShadePixel(shadowFactor);

            var lightmapStartX = lightmapUnitSize * faceIndex;
            var lightmapStartY = row * lightmapUnitSize;
            var lightmapEndX = lightmapStartX + lightmapUnitSize;
            var lightmapEndY = lightmapStartY + lightmapUnitSize;

            for (var y = lightmapStartY; y < lightmapEndY; y++)
            {
                for (var x = lightmapStartX; x < lightmapEndX; x++)
                {
                    var pixelToTex = new Vector2(x / (float) textureWidth, y / (float) textureHeight);

                    var pixelIsInsideReceiver = PointIsInsideTriangle(receiverTextureCoordinates, pixelToTex);

                    if (!lightCache.Pixels[y, x])
                    {
                        lightmapTexture.SetPixel(x, y, diffuseColor);
                    }

                    if (!pixelIsInsideReceiver || isOnlyDiffuseLighting)
                    {
                        continue;
                    }

                    var pixelIsInsideProjector = PointIsInsideTriangle(projectorToPlane, pixelToTex);

                    if (!pixelIsInsideProjector)
                    {
                        continue;
                    }

                    lightmapTexture.SetPixel(x, y, shadowColor);
                    lightCache.Pixels[y, x] = true;
                }
            }
        }

        private static Color ShadePixel(float shadowFactor)
        {
            var red = LightColor.R * shadowFactor * 255.0f;

            var green = LightColor.G * shadowFactor * 255.0f;

            var blue = LightColor.B * shadowFactor * 255.0f;

            return Color.FromArgb((int) red, (int) green, (int) blue);
        }

        private static Vector2 GetMin(IEnumerable<Vector2> points)
        {
            var minimumX = float.PositiveInfinity;
            var minimumY = float.PositiveInfinity;

            foreach (var point in points)
            {
                if (point.X < minimumX)
                {
                    minimumX = point.X;
                }

                if (point.Y < minimumY)
                {
                    minimumY = point.Y;
                }
            }

            return new Vector2(minimumX, minimumY);
        }

        private static Vector2 GetMax(IEnumerable<Vector2> points)
        {
            var maximumX = float.NegativeInfinity;
            var maximumY = float.NegativeInfinity;

            foreach (var point in points)
            {
                if (point.X > maximumX)
                {
                    maximumX = point.X;
                }

                if (point.Y > maximumY)
                {
                    maximumY = point.Y;
                }
            }

            return new Vector2(maximumX, maximumY);
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        private static Axis IsHigher(Vector3 vector)
        {
            var squaredX = vector.X * vector.X;

            var squaredY = vector.Y * vector.Y;

            var squaredZ = vector.Z * vector.Z;

            if (squaredX >= squaredY && squaredX >= squaredZ)
            {
                return Axis.X;
            }

            if (squaredZ >= squaredX && squaredZ >= squaredY)
            {
                return Axis.Z;
            }

            return Axis.Y;
        }

        private static bool RayIsIntersectsPlane(Vector3 planeCenter, Vector3 planeNormal, Vector3 rayStart,
            Vector3 rayDirection, out Vector3 hitPoint)
        {
            const float epsilon = float.Epsilon;

            var normalDotRayDirection = Vector3.Dot(planeNormal.Normalized(), rayDirection);

            hitPoint = rayStart + rayDirection;

            if (Math.Abs(normalDotRayDirection) < epsilon)
            {
                return false;
            }

            var t = Vector3.Dot(planeCenter - rayStart, planeNormal) / normalDotRayDirection;

            if (t < 0)
            {
                return false;
            }

            hitPoint = rayStart + rayDirection * t;

            return true;
        }

        private static bool PointIsInsideTriangle(IReadOnlyList<Vector2> trianglePoints, Vector2 testPoint)
        {
            var a = trianglePoints[0];

            var b = trianglePoints[1];

            var c = trianglePoints[2];

            var signA = Sign(new[]
            {
                testPoint,
                a,
                b
            });

            var signB = Sign(new[]
            {
                testPoint,
                b,
                c
            });

            var signC = Sign(new[]
            {
                testPoint,
                c,
                a
            });

            var hasNegative = signA < 0.0f || signB < 0.0f || signC < 0.0f;
            var hasPositive = signA > 0.0f || signB > 0.0f || signC > 0.0f;

            return !(hasNegative && hasPositive);
        }

        private static float Sign(IReadOnlyList<Vector2> points)
        {
            var a = points[0];

            var b = points[1];

            var c = points[2];

            return (a.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - c.Y);
        }

        public static Face[] LoadMesh(string fileName)
        {
            var outFaces = new List<Face>();

            var importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            var scene = importer.ImportFile(fileName,
                PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);
            var mesh = scene.Meshes[0];
            var inFaces = mesh.Faces;

            foreach (var face in inFaces)
            {
                const int vertexCount = 3;

                var vertices = new Vertex[vertexCount];

                for (var i = 0; i < vertexCount; i++)
                {
                    var index = face.Indices[i];
                    var vertexOrigin = mesh.Vertices[index].ToOpenTK();
                    var normal = mesh.Normals[index].ToOpenTK();
                    var uv = mesh.TextureCoordinateChannels[0][index].ToOpenTK().Xy;

                    vertices[i] = new Vertex
                    {
                        WorldLocation = vertexOrigin,
                        Normal = normal,
                        BaseUV = uv
                    };
                }

                outFaces.Add(new Face(vertices[0], vertices[1], vertices[2]));
            }

            return outFaces.ToArray();
        }

        #region [ Save / Load ]

        private const string ModelDataPrefix = "ModelData";

        private const string FacePrefix = "Face";
        private const string VertexPrefix = "Vertex";
        private const string WorldPositionPrefix = "WorldLocation";
        private const string UV0Prefix = "UV0";
        private const string UV1Prefix = "UV1";

        public static void ExportXml(string fileName, Mesh mesh)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(ModelDataPrefix);
                var faces = mesh.Faces;
                foreach (var face in faces)
                {
                    WriteFaceData(writer, face);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        private static void WriteFaceData(XmlWriter writer, Face face)
        {
            writer.WriteStartElement(FacePrefix);
            for (var v = 0; v < FaceVertexCount; v++)
            {
                WriteVertexData(writer, face[v]);
            }

            writer.WriteEndElement();
        }

        private static void WriteVertexData(XmlWriter writer, Vertex vertex)
        {
            var worldLocation = vertex.WorldLocation;
            var uv0 = vertex.BaseUV;
            var uv1 = vertex.LightmapUV;

            writer.WriteStartElement(VertexPrefix);
            writer.WriteElementString(WorldPositionPrefix, $"{worldLocation.X} {worldLocation.Y} {worldLocation.Z}");
            writer.WriteElementString(UV0Prefix, $"{uv0.X} {uv0.Y}");
            writer.WriteElementString(UV1Prefix, $"{uv1.X} {uv1.Y}");
            writer.WriteEndElement();
        }

        public static Mesh ImportXml(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);

            var root = doc.DocumentElement;

            var faces = new List<Face>();

            foreach (XmlElement upperData in root)
            {
                var vertices = new List<Vertex>();
                foreach (XmlNode lowerData in upperData.ChildNodes)
                {
                    var vertex = new Vertex();
                    foreach (XmlNode vertexData in lowerData.ChildNodes)
                    {
                        switch (vertexData.Name)
                        {
                            case WorldPositionPrefix:
                                vertex.WorldLocation = ParsePosition(vertexData.InnerText);
                                break;
                            case UV0Prefix:
                                vertex.BaseUV = ParseUV(vertexData.InnerText);
                                break;
                            case UV1Prefix:
                                vertex.LightmapUV = ParseUV(vertexData.InnerText);
                                break;
                        }
                    }

                    vertices.Add(vertex);
                }

                faces.Add(new Face(vertices[0], vertices[1], vertices[2]));
            }

            var meshVertices = new List<Vector3>();
            var meshBaseUVs = new List<Vector2>();
            var meshLightmapUVs = new List<Vector2>();
            var meshIndices = new List<int>();

            var index = 0;
            foreach (var face in faces)
            {
                for (var v = 0; v < FaceVertexCount; v++)
                {
                    var vertex = face[v];
                    meshVertices.Add(vertex.WorldLocation);
                    meshBaseUVs.Add(vertex.BaseUV);
                    meshLightmapUVs.Add(vertex.LightmapUV);
                    meshIndices.Add(index + v);
                }

                index += FaceVertexCount;
            }

            return new Mesh(
                faces.ToArray(),
                meshVertices.ToArray(),
                meshBaseUVs.ToArray(),
                meshLightmapUVs.ToArray(),
                meshIndices.ToArray());
        }

        private static Vector3 ParsePosition(string source)
        {
            var data = source.Split(' ');
            Vector3 outVector;
            outVector.X = float.Parse(data[0]);
            outVector.Y = float.Parse(data[1]);
            outVector.Z = float.Parse(data[2]);
            return outVector;
        }

        private static Vector2 ParseUV(string source)
        {
            var data = source.Split(' ');
            Vector2 outVector;
            outVector.X = float.Parse(data[0]);
            outVector.Y = float.Parse(data[1]);
            return outVector;
        }

        #endregion

        public class LightCache
        {
            public LightCache(Image lightmapTexture)
            {
                var width = lightmapTexture.Width;

                var height = lightmapTexture.Height;

                Pixels = new bool[height, width];

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        Pixels[y, x] = false;
                    }
                }
            }

            public bool[,] Pixels { get; }
        }

        public class Vertex
        {
            public Vector3 WorldLocation { get; set; }

            public Vector2 BaseUV { get; set; }

            public Vector2 LightmapUV { get; set; }

            public Vector3 Normal { get; set; }
        }

        public class Face
        {
            public Face(Vertex a, Vertex b, Vertex c)
            {
                A = a;
                B = b;
                C = c;
            }

            public Vertex this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return A;
                        case 1:
                            return B;
                        case 2:
                            return C;
                    }

                    throw new Exception("Index out of range");
                }
            }

            public Vertex A { get; set; }

            public Vertex B { get; set; }

            public Vertex C { get; set; }
        }
    }
}