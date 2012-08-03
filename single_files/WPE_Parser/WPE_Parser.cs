using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace WeProgram_Mail {
    class WPE_Parser {


        public void errorMessage(string error_message, string error_program) {
            MessageBox.Show(error_message + "\n\n" + error_program);
        }

        public string get_exe_path() {
            string exe_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + Path.DirectorySeparatorChar;
            exe_path = exe_path.Replace(@"file:\", "");
            exe_path = exe_path.Replace(@"\\", @"\");
            return exe_path;
        }

        public Boolean check_if_file_exist(string file_name) {
            string execution_path = get_exe_path();
            string file_to_search = execution_path + file_name;
            if (File.Exists(file_to_search)) {
                return true;
            }
            return false;
        }

        public Boolean make_file(string file_name) {
            string execution_path = get_exe_path();
            string file_to_make = execution_path + file_name;
            try {
                File.Create(file_to_make);
            } catch (Exception e) {
                errorMessage("Er is een fout opgetreden in het maken van de Database van de gebruikers.", e.ToString());
            }
            return check_if_file_exist(file_name);
        }

        public Boolean search_for_name(string file_name, string group_name) {
            string execution_path = get_exe_path();
            string file_to_read = execution_path + file_name;
            string[] lines = System.IO.File.ReadAllLines(file_to_read);
            string value_of_line = "";
            int counter_lines = lines.Length;
            int counter_wich_line = 0;

            while (counter_wich_line < counter_lines) {
                value_of_line = lines[counter_wich_line].Trim();
                if (value_of_line == "<name>") {
                    counter_wich_line++;
                    value_of_line = lines[counter_wich_line].Trim();
                    if (value_of_line == group_name) {
                        return true;
                    }
                }
                counter_wich_line++;
            }

            return false;
        }

        public Boolean write_to_file_non_array(string file_name, string name, string type, string[] data) {
            try {
                Boolean already_exist = search_for_name(file_name, name);
                if (already_exist == true) {
                    errorMessage("De database kon niet worden bijgewerkt.", "Er bestaat al een groep met de naam " + name);
                    return false;
                }
                string execution_path = get_exe_path();
                string file_to_write = execution_path + file_name;
                System.IO.StreamWriter file = new System.IO.StreamWriter(file_to_write, true);
                file.WriteLine("\n<group>");
                file.WriteLine("\t<name>");
                file.WriteLine("\t\t" + name);
                file.WriteLine("\t</name>");
                file.WriteLine("\t<type>");
                file.WriteLine("\t\t" + type);
                file.WriteLine("\t</type>");
                file.WriteLine("\t<value>");
                foreach (string s in data) {
                    file.WriteLine("\t\t" + s);
                }
                file.WriteLine("\t</value>");
                file.WriteLine("</group>");
                file.Close();
            } catch (Exception e) {
                errorMessage("De database kon niet worden bijgewerkt.", e.ToString());
            }
            return true;
        }

        public Boolean write_to_file_single_array(string file_name, string name, string type, string[,] data) {
            try {
                Boolean already_exist = search_for_name(file_name, name);
                if (already_exist == true) {
                    errorMessage("De database kon niet worden bijgewerkt.", "Er bestaat al een groep met de naam " + name);
                    return false;
                }
                string execution_path = get_exe_path();
                string file_to_write = execution_path + file_name;
                System.IO.StreamWriter file = new System.IO.StreamWriter(file_to_write, true);
                file.WriteLine("\n<group>");
                file.WriteLine("\t<name>");
                file.WriteLine("\t\t" + name);
                file.WriteLine("\t</name>");
                file.WriteLine("\t<type>");
                file.WriteLine("\t\t" + type);
                file.WriteLine("\t</type>");
                file.WriteLine("\t<value>");
                // index 0 of arry must be the max index (5, 10) and seperate then on {,}
                string indexes = data[0, 0];
                string[] splitted_indexes = indexes.Split(',');
                int index_max_1 = Convert.ToInt32(splitted_indexes[0]);
                int index_max_2 = Convert.ToInt32(splitted_indexes[1]);
                int count_index_1 = 1;
                int count_index_2 = 0;
                while (count_index_1 <= index_max_1) {
                    file.WriteLine("\t\t<sub>");
                    while (count_index_2 < index_max_2) {
                        file.WriteLine("\t\t\t<index>");
                        file.WriteLine("\t\t\t\t" + count_index_2);
                        file.WriteLine("\t\t\t</index>");
                        file.WriteLine("\t\t\t<value>");
                        file.WriteLine("\t\t\t\t" + data[count_index_1,count_index_2]);
                        file.WriteLine("\t\t\t</value>");
                        count_index_2++;
                    }
                    file.WriteLine("\t\t</sub>");
                    count_index_1++;
                }
                file.WriteLine("\t</value>");
                file.WriteLine("</group>");
                file.Close();
            } catch (Exception e) {
                errorMessage("De database kon niet worden bijgewerkt.", e.ToString());
            }
            return true;
        }

        public string[] get_all_lines(string file_name) {
            string execution_path = get_exe_path();
            string file_to_read = execution_path + file_name;
            string[] lines = System.IO.File.ReadAllLines(file_to_read);
            return lines;
        }

        private string[,] parse_the_lines(string group_name, string[] alle_regels) {
            int is_in_group = 0;
            int is_in_name = 0;
            int counter_lines = alle_regels.Length;
            int counter_wich_line = 0;
            string type_of_value = "";
            string value_of_line = "";
            string[,] inhoud_gewild = new string[alle_regels.Length, alle_regels.Length];
            string data_needed = "";

            while (counter_wich_line < counter_lines) {
                value_of_line = alle_regels[counter_wich_line].Trim();
                if (is_in_group != 1) {
                    if (value_of_line == "<group>") {
                        is_in_group = 1;
                        int counter_1 = counter_wich_line;
                        while (counter_1 < counter_lines) {
                            value_of_line = alle_regels[counter_1].Trim();
                            if (is_in_name != 1) {
                                if (value_of_line == "<name>") {
                                    is_in_name = 1;
                                    counter_1++;
                                    value_of_line = alle_regels[counter_1].Trim();
                                    if (value_of_line != group_name) {
                                        int counter_2 = counter_1;
                                        is_in_group = 0;
                                        is_in_name = 0;
                                        while (counter_2 < counter_lines) {
                                            value_of_line = alle_regels[counter_2].Trim();
                                            if (value_of_line == "</group>") {
                                                counter_wich_line = counter_2;
                                                counter_2 = counter_lines;
                                            }
                                            counter_2++;
                                        }
                                    } else if (value_of_line == group_name) {
                                        int counter_3 = counter_1;
                                        int counter_4 = 0;
                                        while (counter_3 < counter_lines) {
                                            value_of_line = alle_regels[counter_3].Trim();
                                            if (value_of_line == "<type>") {
                                                counter_3++;
                                                value_of_line = alle_regels[counter_3].Trim();
                                                type_of_value = value_of_line;
                                                counter_4 = counter_3;
                                                counter_3 = counter_lines;
                                            }
                                            counter_3++;
                                        }
                                        if (type_of_value == "array" || type_of_value == "multie_array") {
                                            /*
                                             * 
                                             * HIER DAN EEN ARRAY PARSEN!!!
                                             * 
                                             */
                                        } else {
                                            while (counter_4 < counter_lines) {
                                                value_of_line = alle_regels[counter_4].Trim();
                                                if (value_of_line == "<value>") {
                                                    counter_4++;    // HIER DAN ZORGN DAT DE DATA WORDT OPGESLAGEN ENDAT ZOORT DINGEN@!@@
                                                    value_of_line = alle_regels[counter_4].Trim();
                                                    data_needed += value_of_line;
                                                    int counter_5 = counter_4 + 1;
                                                    while (counter_5 < counter_lines) {
                                                        value_of_line = alle_regels[counter_5].Trim();
                                                        if (value_of_line == "</value>") {
                                                            counter_5 = counter_lines;
                                                        } else {
                                                            data_needed += "\n" + value_of_line;
                                                        }

                                                        counter_5++;
                                                    }
                                                    counter_4 = counter_lines;
                                                }
                                                counter_4++;
                                            }
                                            inhoud_gewild[0, 0] = data_needed;
                                        }
                                    }
                                }
                            }
                            counter_1++;
                        }
                    }
                }
                counter_wich_line++;
            }

            return inhoud_gewild;
        }

        public string[,] get_inhoud(string file_name, string group_name) {
            Boolean file_exist = check_if_file_exist(file_name);
            if (file_exist == false) {
                Boolean file_maked = make_file(file_name);
            } else {

            }
            string[] alle_regels = get_all_lines(file_name);
            string[,] gewilde_inhoud = parse_the_lines(group_name, alle_regels);
            return gewilde_inhoud;
        }

        public void start_all() {
            string execution_path = get_exe_path();
        }
    }
}
