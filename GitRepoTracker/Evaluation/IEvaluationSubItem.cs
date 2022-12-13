using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public interface IEvaluationSubItem
    {
        public string Html(StudentGroup group);
    }
}
