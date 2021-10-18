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

/*
 * Created by SharpDevelop.
 * User: Markus
 * Date: 30.07.2016
 * Time: 17:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace GralIO
{
    /// <summary>
    /// Class to read files from computation folder
    /// </summary>
    public class IO_ReadFiles
    {
        //User defineddecimal seperator
        private string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        
        private string _projectname;
        public string ProjectName { set {_projectname = value;} }
        private string _emissionfile;
        public string EmissionFile { set {_emissionfile = value;} }
        private string _roughness;
        public string Roughness {get {return _roughness;} }
        private decimal _GRAMMstartsituation;
        public decimal GRAMMstartsituation {get {return _GRAMMstartsituation;} }
        private string _GRAMMsmooth = "";
        public string GRAMMsmooth {get {return _GRAMMsmooth;} }
        private string _unit = "";
        public string Unit {get {return _unit;} }
        private bool _GRAMMsteadystate;
        public bool GRAMMsteadystate {get {return _GRAMMsteadystate;} }
        private int _GRAMMsunrise;
        public int GRAMMsunrise {get {return _GRAMMsunrise;} }
        private int _GRALTrackbar;
        public int GRALTrackbar {get {return _GRALTrackbar;} }
        private int _GRAMMTrackbar;
        public int GRAMMTrackbar {get {return _GRAMMTrackbar;} }
        private bool _MeteoClassification;
        public bool MeteoClassification {get {return _MeteoClassification;} }
        private double _GRAMMnumbcell;
        public double GRAMMnumbcell {get {return _GRAMMnumbcell;} }
        private decimal _GRAMMvertlayers;
        public decimal GRAMMvertlayers {get {return _GRAMMvertlayers;} }
        
        private Int32 _GRAMMTimeStep;
        public Int32 GRAMMTimeStep {get {return _GRAMMTimeStep;} }
        private Int32 _GRAMMModellingTime;
        public Int32 GRAMMModellingTime {get {return _GRAMMModellingTime;} }
        private decimal _GRAMMRelaxvel;
        public decimal GRAMMRelaxvel {get {return _GRAMMRelaxvel;} }
        private decimal _GRAMMRelaxscal;
        public decimal GRAMMRelaxscal {get {return _GRAMMRelaxscal;} }
        private Int32 _GRAMMDebugLevel;
        public Int32 GRAMMDebugLevel {get {return _GRAMMDebugLevel;} }
        private bool _GRAMMCatabatic;
        public bool GRAMMCatabatic { get { return _GRAMMCatabatic; } }
        private bool _GRAMMBoundaryCondition;
        public bool GRAMMBoundaryCondition { get { return _GRAMMBoundaryCondition; } }
        
        private double[] _dispsituationfrequ;
        public double[] DispsituationFrequ {set {_dispsituationfrequ = value;}  get {return _dispsituationfrequ;} }
        private double _meteofrequ;
        public double MeteoFrequ {get {return _meteofrequ;} }
        
        private decimal _flowfieldgrid;
        public decimal FlowfieldGrid {get {return _flowfieldgrid;} }
        private decimal _flowfieldvertical;
        public decimal FlowfieldVertical {get {return _flowfieldvertical;} }
        private decimal _flowfieldstretch;
        public decimal FlowfieldStretch {get {return _flowfieldstretch;} }

        public List<float[]> FlowFieldStretchFlexible = new List<float[]>();
        
        private int _cellsgralx;
        public int CellsGralX {get {return _cellsgralx;} }
        private int _cellsgraly;
        public int CellsGralY {get {return _cellsgraly;} }
        
        private int _numhorslices;
        public int NumhorSlices {get {return _numhorslices;} }
        
        private string[] _sourcegrp;
        public string[] Source_group_list {set {_sourcegrp = value;}  get {return _sourcegrp;} }
        
        private decimal _flowfieldmicrolayers;
        public decimal FlowfieldMicrolayers {get {return _flowfieldmicrolayers;} }
        private decimal _relaxvel;
        public decimal RelaxVel {get {return _relaxvel;} }
        private decimal _relaxpress;
        public decimal RelaxPress {get {return _relaxpress;} }
        private decimal _buildingrough;
        public decimal BuildingRough {get {return _buildingrough;} }
        private decimal _integrationmin;
        public decimal IntegrationMin {get {return _integrationmin;} }
        private decimal _integrationmax;
        public decimal IntegrationMax {get {return _integrationmax;} }
        
        private double _west;
        public double West {get {return _west;} }
        private double _east;
        public double East {get {return _east;} }
        private double _north;
        public double North {get {return _north;} }
        private double _south;
        public double South {get {return _south;} }
        
        private List<Gral.SG_Class> _source_group_list = new List<Gral.SG_Class>();   
        public List<Gral.SG_Class> Source_Group_List { get {return _source_group_list;} }
        
        private List<string> _sourcedata = new List<string>();   
        public List<string> SourceData { set {_sourcedata = value;} get {return _sourcedata;} }
        
        private string _winddatafile;
        public string WindDataFile { set {_winddatafile = value;} }
        private List<GralData.WindData> _winddata;
        public List<GralData.WindData> WindData { set { _winddata = value; } get { return _winddata; } }

        private decimal _transConcThreshold;
        public decimal TransConcThreshold { get { return _transConcThreshold; } }

        private bool _GRAMMERA5;
        public bool GRAMMERA5 { get { return _GRAMMERA5; } }
        private string _GRAMMERA5DateTime;
        public string GRAMMERA5DateTime { get { return _GRAMMERA5DateTime; } }

        /// <summary>
        /// Read GRAMMin.dat file
        /// </summary>
        public bool ReadGrammInFile()
        {
            if (_projectname.Length == 0 )
            {
                return false;
            }

            if (File.Exists(Path.Combine(_projectname, @"Computation","GRAMMin.dat")))
            {
                try
                {
                    string dummy;
                    string[] text5 = new string[10];
                    using (StreamReader myreader = new StreamReader(Path.Combine(_projectname, @"Computation","GRAMMin.dat")))
                    {
                        dummy=myreader.ReadLine();
                        _GRAMMsunrise = 0; // no sunrise option
                        if (dummy.Contains("Version") == true)
                        {
                            dummy=myreader.ReadLine(); // yes
                            _roughness = myreader.ReadLine(); // roughness lenght
                            dummy=myreader.ReadLine(); // start number "," bound cells
                            text5 = dummy.Split(new char[] { ' ', ',', '\t', ';', '!' }, StringSplitOptions.RemoveEmptyEntries);
                            if (text5.Length >1)
                            {
                                _GRAMMstartsituation = Convert.ToInt32(text5[0]);
                                _GRAMMsmooth = text5[1];
                            }
                            // steady state files
                            if (myreader.EndOfStream == false)
                            {
                                dummy=myreader.ReadLine(); // yes/no: write steady-state file
                                if (dummy.Contains("yes"))
                                {
                                    _GRAMMsteadystate = true;
                                }
                                else
                                {
                                    _GRAMMsteadystate = false;
                                }
                            }
                            // sunrise option
                            if (myreader.EndOfStream == false)
                            {
                                dummy=myreader.ReadLine(); // sunrise option
                                text5 = dummy.Split(new char[] { ' ', ',', '\t', ';', '!' }, StringSplitOptions.RemoveEmptyEntries);
                                if (text5.Length >= 0)
                                {
                                    _GRAMMsunrise = Convert.ToInt32(text5[0]);
                                }
                            }
                        }
                        else
                        {
                            _roughness = myreader.ReadLine();
                            _GRAMMstartsituation = Convert.ToInt32(myreader.ReadLine());
                        }
                    }
                    return true;
                }
                catch
                { 
                    return false; // error reading this file
                }
            }
                return false;
        }

        /// <summary>
        /// Read GRAL_Trans_Conc_Threshold.txt file
        /// </summary>
        public void ReadTransConcThreshold()
        {
            if (File.Exists(Path.Combine(_projectname, @"Computation", "GRAL_Trans_Conc_Threshold.txt")))
            {
                try
                {
                    using (StreamReader myreader = new StreamReader(Path.Combine(_projectname, @"Computation", "GRAL_Trans_Conc_Threshold.txt")))
                    {
                        _transConcThreshold = Convert.ToDecimal(myreader.ReadLine().Replace(".", decsep));
                    }
                }
                catch
                {
                    _transConcThreshold = Convert.ToDecimal(0.1);
                }
            }
        }
        
        /// <summary>
        /// Read DispNr.txt file
        /// </summary>
        public bool ReadDispNrFile()
        {
            if(File.Exists(Path.Combine(_projectname, @"Computation","DispNr.txt")))
            {
                try
                {
                    using (StreamReader myreader = new StreamReader(Path.Combine(_projectname, @"Computation","DispNr.txt")))
                    {
                        _GRALTrackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", "")) - 1;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Read DispNrGramm.txt file
        /// </summary>
        public bool ReadDispGrammNrFile()
        {
            if(File.Exists(Path.Combine(_projectname, @"Computation","DispNrGramm.txt")))
            {
                try
                {
                    using (StreamReader myreader = new StreamReader(Path.Combine(_projectname, @"Computation","DispNrGramm.txt")))
                    {
                        _GRAMMTrackbar = Convert.ToInt32(myreader.ReadLine().Replace(" ", "")) - 1;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Read Sourcegroups.txt file
        /// </summary>
        public bool ReadSourcegroupsTextFile()
        {
            try
            {
                string newPath1 = Path.Combine(_projectname, @"Settings","Sourcegroups.txt");
                if (File.Exists(newPath1))
                {
                    using (StreamReader myReader = new StreamReader(newPath1))
                    {
                        string text1;
                        string[] text = new string[2];
                        _source_group_list.Clear();
                        
                        text1 = myReader.ReadLine();
                        while (text1 != null)
                        {
                            text = text1.Split(new char[] { ',' });
                            if (text.Length > 1)
                            {
                                int _sg = 0;
                                if (int.TryParse(text[1], out _sg))
                                {
                                    _source_group_list.Add(new Gral.SG_Class() {SG_Name = text[0], SG_Number = _sg});
                                }
                            }
                            text1 = myReader.ReadLine();
                        }
                    }
                    
                    _source_group_list.Sort();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read GRAL.geb file
        /// </summary>
        public bool ReadGRALGebFile()
        {
            try
            {
                string newPath2 = Path.Combine(_projectname, @"Computation","GRAL.geb");
                using (StreamReader myreader = new StreamReader(newPath2))
                {
                    string[] text = new string[10];
                                    
                    text = myreader.ReadLine().Split(new char[] { '!' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    
                    _flowfieldgrid = Convert.ToDecimal(text[0]);
                    
                    text = myreader.ReadLine().Split(new char[] { '!' });

                    text = myreader.ReadLine().Split(new char[] {'!' }); // Remove Comment
                    text = text[0].Split(new char[] { ',' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    text[1] = text[1].Trim();
                    text[1] = text[1].Replace(".", decsep);
                    _flowfieldvertical = Convert.ToDecimal(text[0].Replace(".", decsep));
                    _flowfieldstretch = Convert.ToDecimal(text[1].Replace(".", decsep));
                    FlowFieldStretchFlexible.Clear();

                    if (text.Length > 2)
                    {
                        for (int i = 2; i < text.Length; i += 2)
                        {
                            FlowFieldStretchFlexible.Add(new float[2]);
                            text[i] = text[i].Trim();
                            text[i] = text[i].Replace(".", decsep);
                            FlowFieldStretchFlexible[FlowFieldStretchFlexible.Count - 1][0] = Convert.ToSingle(text[i]); // Height
                            
                            text[i + 1] = text[i + 1].Trim();
                            text[i + 1] = text[i + 1].Replace(".", decsep);
                            FlowFieldStretchFlexible[FlowFieldStretchFlexible.Count - 1][1] = Convert.ToSingle(text[i + 1]); //Stretch
                        }
                    }

                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    _cellsgralx = Convert.ToInt32(text[0]);
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    _cellsgraly = Convert.ToInt32(text[0]);
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    _numhorslices = Convert.ToInt32(text[0]);
                    
                    _sourcegrp = myreader.ReadLine().Split(new char[] { ',', '!' }, StringSplitOptions.RemoveEmptyEntries);
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _west = Convert.ToDouble(text[0].Replace(".", decsep));
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _east = Convert.ToDouble(text[0].Replace(".", decsep));
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _south = Convert.ToDouble(text[0].Replace(".", decsep));
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _north = Convert.ToDouble(text[0].Replace(".", decsep));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read micro_vert_layers.txt file
        /// </summary>
        public bool ReadMicroVertLayers()
        {
            try
            {
                string newPath1 = Path.Combine(_projectname, @"Computation","micro_vert_layers.txt");
                using (StreamReader myreader = new StreamReader(newPath1))
                {
                    string text = myreader.ReadLine().Replace(".",decsep);
                    _flowfieldmicrolayers = Convert.ToDecimal(text);
                }
            
                return true;
            }
            catch 
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read relaxation_factors.txt file
        /// </summary>
        public bool ReadRelaxFactorsFile()
        {
            try
            {
                string newPath1 = Path.Combine(_projectname, @"Computation","relaxation_factors.txt");
                using (StreamReader myreader = new StreamReader(newPath1))
                {
                    string text = myreader.ReadLine().Replace(".",decsep);
                    _relaxvel = Convert.ToDecimal(text); //vel
                    text = myreader.ReadLine().Replace(".",decsep);
                    _relaxpress = Convert.ToDecimal(text); //pres
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read building_roughness.txt file
        /// </summary>
        public bool ReadBuildingRoughnessFile()
        {
            try
            {
                string newPath1 = Path.Combine(_projectname, @"Computation","building_roughness.txt");
                using (StreamReader myreader = new StreamReader(newPath1))
                {
                    string text = myreader.ReadLine().Replace(".",decsep);
                    _buildingrough = Convert.ToDecimal(text);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read Integrationtime.txt file
        /// </summary>
        public bool ReadIntegrationTimeFile()
        {
            try
            {
                string newPath1 = Path.Combine(_projectname, @"Computation","Integrationtime.txt");
                using (StreamReader myreader = new StreamReader(newPath1))
                {
                    string text = myreader.ReadLine().Replace(".", decsep);
                    _integrationmin = Convert.ToDecimal(text);  //min
                    text = myreader.ReadLine().Replace(".",decsep); 
                    _integrationmax = Convert.ToDecimal(text); //max
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read GRAMM.geb file
        /// </summary>
        public bool ReadGrammGebFile()
        {
            try
            {
                string newPath2 = Path.Combine(_projectname, @"Computation","GRAMM.geb");
                using (StreamReader myreader = new StreamReader(newPath2))
                {
                    string[] text = new string[10];
                    _GRAMMnumbcell = 1;
                    text = myreader.ReadLine().Split(new char[] { '!' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    _GRAMMnumbcell = Convert.ToDouble(text[0]);
                    
                    text = myreader.ReadLine().Split(new char[] { '!' });
                    text = myreader.ReadLine().Split(new char[] { '!' });
                    text[0] = text[0].Trim();
                    text[0] = text[0].Replace(".", decsep);
                    _GRAMMvertlayers = Convert.ToDecimal(text[0]);
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _west = Convert.ToDouble(text[0].Replace(".", decsep));
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _east = Convert.ToDouble(text[0].Replace(".", decsep));
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _south = Convert.ToDouble(text[0].Replace(".", decsep));
                    
                    text = myreader.ReadLine().Split(new char[] { ',', '!' });
                    text[0] = text[0].Trim();
                    _north = Convert.ToDouble(text[0].Replace(".", decsep));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read meteopgt.all file
        /// </summary>
        public bool ReadMeteopgtAllFile()
        {
            try
            {
                List<string> text1 = new List<string>(); // save meteopgt.all lines temporarily
                bool meteo_header_new = false;
                double _meteofrequ_max = -100000000;
                double _meteofrequ_min = 100000000;

                using (StreamReader meteopgt = new StreamReader(Path.Combine(_projectname, @"Computation","meteopgt.all")))
                {
                    string[] text = new string[1];
                    //Header line 1
                    string temp = meteopgt.ReadLine();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        text1.Add(temp);
                        text = text1[0].Split(new char[] { ' ', ',', '\t', ';', '!' }, StringSplitOptions.RemoveEmptyEntries);

                        try
                        {
                            text[1] = text[1].Trim();
                            if (text[1] == "1")
                            {
                                _MeteoClassification = true; // no classification
                            }
                            else
                            {
                                _MeteoClassification = false;
                            }
                        }
                        catch
                        {
                            _MeteoClassification = false;
                        }
                        //Header line 2
                        temp = meteopgt.ReadLine();
                        if (!string.IsNullOrEmpty(temp))
                        {
                            text1.Add(temp);

                            _meteofrequ = 0;
                            double _temp = 0;
                            if (_dispsituationfrequ == null)
                            {
                                _dispsituationfrequ = new double[100000];
                            }
                            for (int n = 0; n < 100000; n++)
                            {
                                try
                                {
                                    if (meteopgt.EndOfStream)
                                    {
                                        break;
                                    }
                                    temp = meteopgt.ReadLine();
                                    if (!string.IsNullOrEmpty(temp))
                                    {
                                        text1.Add(temp);
                                        text = text1[n + 2].Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                        _temp = Convert.ToDouble(text[3].Replace(".", decsep));
                                        _meteofrequ += _temp;
                                        _meteofrequ_max = Math.Max(_meteofrequ_max, _temp);
                                        _meteofrequ_min = Math.Min(_meteofrequ_min, _temp);
                                        _dispsituationfrequ[n] = _temp;
                                    }
                                }
                                catch
                                {
                                    break;
                                }
                            }
                        }
                    }
                    // if the flag for the classification does not match the data (at matches from GUI versions below 18.01) correct the flag
                    if (_MeteoClassification == true &&
                        Math.Abs(_meteofrequ_max - _meteofrequ_min) > _meteofrequ_min) // different freuquencies -> classified meteo!
                    {
                        _MeteoClassification = false; // classified meteo
                        meteo_header_new = true;
                    }                    
                }
                
                if(meteo_header_new)
                {
                    //correct the header in meteopgt.all
                    try
                    {
                        //check if meteopgt was read completely
                        int lines = (int)GralStaticFunctions.St_F.CountLinesInFile(Path.Combine(_projectname, @"Computation", "meteopgt.all"));
                        if (lines == text1.Count)
                        {
                            File.Delete(Path.Combine(_projectname, @"Computation", "meteopgt.all"));
                            using (StreamWriter _meteopgt = new StreamWriter(Path.Combine(_projectname, @"Computation", "meteopgt.all")))
                            {
                                string[] _text = text1[0].Split(new char[] { ' ', ',', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);

                                if (text1.Count > 2)
                                {
                                    _meteopgt.WriteLine(_text[0] + ",0," + _text[2]);
                                }
                                else
                                {
                                    _meteopgt.WriteLine("10,0,10");
                                }

                                for (int n = 1; n < text1.Count; n++)
                                {
                                    _meteopgt.WriteLine(text1[n]);
                                }
                            }
                        }
                    }
                    catch
                    { }
                }

                return true;
            }
            catch
            {
                return false;
            }
        } // Read Meteopgt.all
        
        /// <summary>
        /// Read IIN,dat file
        /// </summary>
        public bool ReadGrammIinFile()
        {
            try
            {
                string newPath2 = Path.Combine(_projectname, @"Computation","IIN.dat");
                using (StreamReader myreader = new StreamReader(newPath2))
                {
                    string[] text = new string[10];
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    _GRAMMERA5DateTime = text[1].Trim(' '); //start date of transient GRAMM simulations                    
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    _GRAMMERA5DateTime += text[1].Trim(' '); //start time of transient GRAMM simulations

                    text = myreader.ReadLine().Split(new char[] { ':','!' });
                    _GRAMMTimeStep = Convert.ToInt32(text[1]); // max Time step
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    _GRAMMModellingTime = Convert.ToInt32(text[1]);   // Modelling time
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    _GRAMMDebugLevel = Convert.ToInt32(text[1]); // Debug level
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    _GRAMMRelaxvel = Convert.ToDecimal(text[1].Replace(".",decsep)); // Relax vel
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    _GRAMMRelaxscal = Convert.ToDecimal(text[1].Replace(".", decsep)); // Relax scal
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    if (Convert.ToInt32(text[1]) > 0)
                    {
                        _GRAMMCatabatic = true; // catabatic forcing
                    }
                    else
                    {
                        _GRAMMCatabatic = false; // catabatic forcing
                    }

                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    if (Convert.ToInt32(text[1]) < 6)
                    {
                        _GRAMMBoundaryCondition = true; // boundary condition for flat terrain
                    }
                    else
                    {
                        _GRAMMBoundaryCondition = false; // boundary condition for flat terrain
                    }

                    text = myreader.ReadLine().Split(new char[] { ':', '!' });
                    if (Convert.ToInt32(text[1]) == 2)
                    {
                        _GRAMMERA5 = true; // large-scale forcing
                    }
                    else
                    {
                        _GRAMMERA5 = false;
                    }
                }

                return true;
            }
            catch 
            {
                return false;
            }
        }
        
        /// <summary>
        /// Read unit from ESRI ASCII File
        /// </summary>		
        public bool ReadRasterUnit()
        {
            if(File.Exists(Path.Combine(_emissionfile)))
            {
                try
                {
                    _unit = _unit = string.Empty;
                    using(StreamReader myReader = new StreamReader(_emissionfile))
                    {
                        string text = myReader.ReadLine(); // read header
                        text = myReader.ReadLine(); // read header
                        text = myReader.ReadLine(); // read header
                        text = myReader.ReadLine(); // read header
                        text = myReader.ReadLine(); // read header
                        text = myReader.ReadLine(); // read header
                        
                        string[] tt = text.Split('\t');
                        if (tt.Length > 2) // found unit entry
                        {
                            _unit = tt[2];
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Read meteo files *.met, *.akterm, *.akt
        /// </summary>	
        public bool ReadMeteoFiles(int File_lenght, char rowsep, string decsep1, string decsepuser, Gral.WindData00Enum Ignore00Values)
        {
            try
            {
                using (StreamReader reader = new StreamReader(_winddatafile))
                {
                    int mode = 0; // read met files
                    if (Path.GetExtension(_winddatafile).ToLower() == ".met")
                    {
                        mode = 0;
                    }
                    else if (Path.GetExtension(_winddatafile).ToLower()  == ".akterm")
                    {
                        mode = 1;
                    }
                    else if (Path.GetExtension(_winddatafile).ToLower() == ".akt")
                    {
                        mode = 2;
                    }

                    string[] zeile;
                    string[] uhrzeit = new string[2];
                    string[] text = new string[50];
                    int counter = 0;
                    double _dirOld = 0;

                    while(reader.EndOfStream == false && counter < File_lenght) // read all lines until file_lenght is reached
                    {
                        string readline = reader.ReadLine();
                        GralData.WindData wd = new GralData.WindData();
                        
                        if (mode == 0) // met files
                        {
                            zeile = readline.Split(rowsep);
                            
                            if (zeile.Length > 3)
                            {
                                uhrzeit = zeile[1].Split(new char[] { ':', '-', '.' });
                                wd.Date = zeile[0];
                                wd.Time = zeile[1];
                                wd.Vel  = Convert.ToDouble(zeile[2].Replace(decsepuser, decsep1));
                                wd.Dir  = Convert.ToDouble(zeile[3].Replace(decsepuser, decsep1));
                                wd.StabClass = Convert.ToInt32(zeile[4]);
                                wd.Hour = Convert.ToInt32(uhrzeit[0]);
                                if (wd.Hour == 24) // if met-file contains 24:00 instead of 00:00
                                {
                                    wd.Hour = 0;
                                }
                                if (wd.Vel >= 0)
                                {
                                    if (wd.Vel < 0.000001 && Ignore00Values == Gral.WindData00Enum.Shuffle00)
                                    {
                                        wd.Dir = _dirOld; // keep previous direction
                                    }
                                    _dirOld = wd.Dir;
                                    if (Ignore00Values != Gral.WindData00Enum.Reject00 || wd.Vel > 0.000001 || wd.Dir > 0.000001)
                                    {
                                        _winddata.Add(wd);
                                    }
                                }
                            }
                        }
                        
                        if (mode == 1) // AKTerm Files
                        {
                            text = readline.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if ((text[0] == "AK") && (text[7] != "9") && (text[8] != "9") && (text[12] != "7") && (text[12] != "9"))
                            {
                                wd.Date = text[4] + "." + text[3] + "." + text[2];
                                wd.Time = text[5] + ":00";
                                wd.Hour  = Convert.ToInt32(text[5]);
                                if (text[8] == "0")
                                {
                                    wd.Vel = Convert.ToDouble(text[10]) * 0.514;
                                }
                                else
                                {
                                    wd.Vel = Convert.ToDouble(text[10].Replace(decsepuser, decsep1)) * 0.1;
                                }

                                if (text[8] == "0")
                                {
                                    wd.Dir = Convert.ToDouble(text[9]) * 10;
                                }
                                else
                                {
                                    wd.Dir = Convert.ToDouble(text[9]);
                                }

                                //Klug-Manier stability classes are transformed to GRAL stability classes in a way to match subsequent Obukhov length calculations
                                wd.StabClass = 8 - Convert.ToInt32(text[12]);
                                if (wd.StabClass < 6)
                                {
                                    wd.StabClass -= 1;
                                }
                                if (wd.Vel >= 0 && wd.Dir < 998)
                                {
                                    if (wd.Vel < 0.000001 && Ignore00Values == Gral.WindData00Enum.Shuffle00)
                                    {
                                        wd.Dir = _dirOld; // keep previous direction
                                    }
                                    _dirOld = wd.Dir;
                                    if (Ignore00Values != Gral.WindData00Enum.Reject00 || wd.Vel > 0.000001 || wd.Dir > 0.000001)
                                    {
                                        _winddata.Add(wd);
                                    }
                                }
                            }
                        }
                        
                        if (mode == 2) // AKT Files
                        {
                            if (readline.Length > 19)
                            {
                                wd.Date = readline.Substring(11,2) + "." + readline.Substring(9,2) + "." + readline.Substring(5,4);
                                wd.Time = readline.Substring(13,2) + ":00";
                                wd.Hour = Convert.ToInt32(readline.Substring(13,2));
                                wd.Vel = Math.Round(Convert.ToDouble(readline.Substring(18,2)) * 0.514,1);
                                wd.Dir = Convert.ToDouble(readline.Substring(16,2)) * 10;
                                //Klug-Manier stability classes are transformed to GRAL stability classes in a way to match subsequent Obukhov length calculations
                                wd.StabClass = 8 - Convert.ToInt32(readline.Substring(20, 1));
                                if (wd.StabClass < 6)
                                {
                                    wd.StabClass -= 1;
                                }
                                if (wd.Vel >= 0 && wd.Dir < 998)
                                {
                                    if (wd.Vel < 0.000001 && Ignore00Values == Gral.WindData00Enum.Shuffle00)
                                    {
                                        wd.Dir = _dirOld; // keep previous direction
                                    }
                                    _dirOld = wd.Dir;
                                    if (Ignore00Values != Gral.WindData00Enum.Reject00 || wd.Vel > 0.000001 || wd.Dir > 0.000001)
                                    {
                                        _winddata.Add(wd);
                                    }
                                }
                            }
                        }

                    } // Loop through the file

                }
                return true;
            }
            catch
            {
                return false;
            }
        }
            
    }
}
