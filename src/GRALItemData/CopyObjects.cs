#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

namespace GralItemData
{
    /// <summary>
    /// Data container for meteopgt.all lines with sortable frequency 
    /// </summary>
    public class CopyObjects
    {
        public PointSourceData PointSource;
        public BuildingData Building;
        public LineSourceData LineSource;
        public ReceptorData Receptor;
        public AreaSourceData AreaSource;
        public PortalsData PortalSource;

        public CopyObjects()
        {
            PointSource = null;
            Building = null;
            LineSource = null;
            Receptor = null;
            AreaSource = null;
            PortalSource = null;
        }
    }
}
