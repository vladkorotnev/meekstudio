using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MeekStudio.Language
{
    public class PvDbEntryTextProcessor
    {
        public PvDbEntryTextProcessor()
        {

        }

        public PvDbEntryTextProcessor(string content)
        {
            Content = content;
        }

        private Dictionary<string, object> _data;
        private Dictionary<string, string> _comments;
        public string Content
        {
            get
            {
                return Formulate(_data);
            }

            set
            {
                Parse(value);
            }
        }

        public Dictionary<string, object> Data
        {
            get
            {
                return _data;
            }
        }

        public Dictionary<string, string> Comments
        {
            get
            {
                return _comments;
            }
        }

        private void Parse(string content)
        {
            _data = new Dictionary<string, object>();
            _comments = new Dictionary<string, string>();
            StringReader r = new StringReader(content);

            string line;
            string curComment = null;
            while ((line = r.ReadLine()) != null)
            {
                if(line.StartsWith("pv_"))
                {
                    var equation = line.Split(new char[] { '=' }, 2);
                    if(curComment != null)
                    {
                        _comments.Add(equation[0], curComment);
                        curComment = null;
                    }
                    if (equation.Count() == 2)
                    {
                        SetByKeyPath(equation[0], equation[1]);
                    }
                }
                else if (line.StartsWith("#"))
                {
                    if(curComment == null)
                    {
                        curComment = line;
                    } 
                    else
                    {
                        curComment += "\r\n"+line;
                    }
                }
            }
        }

        public string KeyPathFromRoot(string keyPath)
        {
            return _data.Keys.First() + "." + keyPath;
        }

        public void SetByKeyPath(string keyPathStr, object value)
        {
            var keyPath = keyPathStr.Split('.');

            Dictionary<string, object> curKeyValue = _data;
            foreach (var key in keyPath)
            {
                if (key == keyPath.Last())
                {
                    curKeyValue[key] = value;
                }
                else
                {
                    if (!curKeyValue.ContainsKey(key))
                    {
                        curKeyValue.Add(key, new Dictionary<string, object>());
                    }

                    curKeyValue = (Dictionary<string, object>)curKeyValue[key];
                }
            }
        }

        public object GetByKeyPath(string keyPathStr)
        {
            var keyPath = keyPathStr.Split('.');

            Dictionary<string, object> curKeyValue = _data;
            foreach (var key in keyPath)
            {
                if (key == keyPath.Last())
                {
                    if(curKeyValue.ContainsKey(key))
                    {
                        return curKeyValue[key];
                    } 
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (!curKeyValue.ContainsKey(key))
                    {
                        return null;
                    }

                    curKeyValue = (Dictionary<string, object>)curKeyValue[key];
                }
            }
            return null;
        }

        private string Formulate(Dictionary<string, object> store, string prefix = "")
        {
            StringBuilder res = new StringBuilder();
            StringWriter w = new StringWriter(res);
            string[] sortedKeys = store.Keys.ToArray();
            Array.Sort(sortedKeys, (x, y) => String.Compare(x, y));
            foreach(string key in sortedKeys)
            {
                object val = store[key];
                if(val is Dictionary<string, object>)
                {
                    w.Write(Formulate((Dictionary<string, object>)val, prefix + key + "."));
                } 
                else
                {
                    if(_comments.ContainsKey(prefix+key))
                    {
                        w.WriteLine(_comments[prefix + key]);
                    }

                    if(key == "length")
                    {
                        val = sortedKeys.Length - 1;
                    }
                    w.WriteLine(String.Format("{0}{1}={2}", prefix, key, val));
                }
            }
            return res.ToString();
        }

        public void SetPvId(int id)
        {
            var backup = _data.Values.First();
            _data.Clear();
            _data.Add("pv_" + id.ToString(), backup);
        }
    }
}
