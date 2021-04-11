#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2020]  [Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion
namespace GralDomain
{
    /// <summary>
    /// Used meteo model 
    /// </summary>
    public enum MeteoModelEmum
    {
        None  = 0b00000000,
        GRAMM = 0b00000001, 
        GRAL  = 0b00000010
    }

    /// <summary>
    /// MouseControl values
    /// </summary>
    public enum MouseMode
    {
        ZoomOut = -1,
        Default = 0,
        ZoomIn = 1,
        ViewMoveMap = 2,
        BaseMapGeoReference1 = 3,
        GralDomainStartPoint = 4,
        GralDomainEndPoint = 5,
        PointSourcePos = 6,
        PointSourceSel = 7,
        AreaSourcePos = 8,
        AreaSourceSel = 9,
        LineSourcePos = 10,
        LineSourceSel = 11,
        BaseMapGeoReference2 = 12,
        ViewPanelZoom = 13,
        ViewPanelZoomArea = 14,
        PortalSourcePos = 15,
        PortalSourceSel = 16,
        BuildingPos = 17,
        BuildingSel = 19,
        ViewNorthArrowPos = 20,
        ViewScaleBarPos = 21,
        ViewDistanceMeasurement = 22,
        ViewAreaMeasurement = 23,
        ReceptorPos = 24,
        ReceptorSel = 25,
        ReceptorDeQueue = 26,
        ViewLegendPos = 28,
        GrammDomainStartPoint = 30,
        GrammDomainEndPoint = 31,
        SetPointMetTimeSeries = 32,
        SetPointConcTimeSeries = 33,
        SetPointSourceApport = 35,
        SetPointVertWindProfileOnline = 40,
        SectionWindSel = 44,
        SectionConcSel = 45,
        SetPointConcFile = 50,
        SetPointVertWindProfile = 62,
        SetPointReOrder = 65,
        SetPointMatch = 66,
        SetPointGRAMMGrid = 70,
        WallSet = 75,
        WallSel = 76,
        VegetationSel = 77,
        WallPosCorner = 78,
        VegetationPosCorner = 79,
        LineSourceEditFinal = 100,
        WallEditFinal = 101,
        AreaSourceEditFinal = 108,
        VegetationEditFinal = 109,
        BuildingEditFinal = 117,
        SetPointConcProfile = 200,
        GrammExportStart = 300,
        GrammExportFinal = 301,
        PointSourceDeQueue = 700,
        LineSourceInlineEdit = 1000,
        WallInlineEdit = 1001,
        AreaInlineEdit = 1080,
        VegetationInlineEdit = 1081,
        BuildingInlineEdit = 1170,
        ReceptorInlineEdit = 2400,
        PointSourceInlineEdit = 6000,
        BaseMapMoveScale = 7000,
        GRALTopographyModify = 9999
    }
}
