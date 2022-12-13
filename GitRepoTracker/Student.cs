using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker
{
    public class Student
    {
        public string Alias { get; set; }
        public List<string> Emails { get; set; } = new List<string>();
    }
}
