using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace face_recognition.Model
{
    class MdlLogFolder
    {
        public int id { get; set; }

        public string name_folder { get; set; }
        public int count_current { get; set; }

        public override string ToString()
        {
            return String.Format("id: {0}, name_folder: {1}, count_current: {2}", id, name_folder, count_current);
        }
    }

    class MdlLogFile
    {
        public string name { get; set; }
        public string path_folder { get; set; }
        public string full_path { get; set; }

        public override string ToString()
        {
            return String.Format("[name: {0}, full_path: {1}]", name, full_path);
        }
    }
}
