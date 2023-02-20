using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class Report
    {
        [XmlElement]
        public string Team { get; set; }

        [XmlElement]
        public StudentGroup Group { get; set; }

        [XmlElement]
        public Commit MasterHeadCommit { get; set; }

        [XmlElement]
        public List<Commit> Commits { get; private set; } = new List<Commit>();

        public Commit LastCommit => Commits.Count > 0 ? Commits[Commits.Count - 1] : null;

        [XmlElement]
        public List<string> UnknowUsers { get; internal set; } = new List<string>();

        [XmlElement]
        public List<string> Images { get; private set; } = new List<string>();

        public Report() { }
        public Report(StudentGroup group)
        {
            Group = group;
            Team = group.Name;
        }

        public static string ToHtml(List<Report> reports, string reportStyleFile)
        {
            string style = null;
            if (System.IO.File.Exists(reportStyleFile))
            {
                style = File.ReadAllText(reportStyleFile);
            }
            string output = $"<html><head><title>Team Reports</title>" +
                $"<script type=\"text/javascript\">function toggle_visibility(id){{var e = document.getElementById(id); if ( e.style.display == 'block' ) e.style.display = 'none'; else e.style.display = 'block'; }}</script></head>" +
                $"<body>" +
                $"<style type=\"text/css\" media=\"screen\">{style}</style>" +
                $"<h1>{Program.Config.Subject}</h1>";

            output += $"<div class=\"reportSubItem\">Updated on {DateTime.Now}</div>";
            output += $"<div class=\"reportSubItem\">Start date: {Program.Config.StartDate}</div>";

            foreach (Report report in reports)
                output += report.ToHtml();

            output += "</body></html>";

            return output;
        }

        public static string CommitToHtmlLink(string gitHubDeveloperAndProject, Commit commit, bool showAuthor = false)
        {
            string commitLink = $"https://github.com/{gitHubDeveloperAndProject}/commit/{commit.Id}";
            if (showAuthor)
                return $"<a href={commitLink}>{commit.Id} ({commit.Author} {commit.Date.Year}/{commit.Date.Month}/{commit.Date.Day})</a>";
            return $"<a href={commitLink}>{commit.Id} ({commit.Date.Year}/{commit.Date.Month}/{commit.Date.Day})</a>";
        }

        public static string CommitLinkedItemToHtmlLink(string gitHubDeveloperAndProject, Evaluation.CommitLinkedItem item)
        {
            string commitLink = $"https://github.com/{gitHubDeveloperAndProject}/commit/{item.Commit.Id}";
            return $"<a href={commitLink}>{item.Item}</a>";
        }

        public static string ToggleSwitch(string text, string divClass, string targetDivId)
        {
            return $"<div class=\"{divClass}\" onClick=\"toggle_visibility('{targetDivId}')\">{text}</div>";
        }

        static Random m_randomGenerator = new Random();
        public static string RandomDivId()
        {
            return m_randomGenerator.Next().ToString();
        }

        public string ToHtml()
        {
            string okPath = "<svg aria-labelledby=\"__bolt-status-131-desc\" style=\"fill: rgba(85,163,98,1)\" height=\"16\" viewBox=\"0 0 16 16\" width=\"16\" xmlns=\"http://www.w3.org/2000/svg\"><desc id=\"__bolt-status-131-desc\">Success 20m 33s</desc><circle cx=\"8\" cy=\"8\" r=\"8\"></circle><path d=\"M6.062 11.144l-.003-.002-1.784-1.785A.937.937 0 1 1 5.6 8.031l1.125 1.124 3.88-3.88A.937.937 0 1 1 11.931 6.6l-4.54 4.54-.004.004a.938.938 0 0 1-1.325 0z\" fill=\"#fff\"></path></svg>";
            string errorPath = "<svg aria-labelledby=\"__bolt-status-104-desc\" style=\"fill: rgba(205,74,69,1)\" height=\"16\" viewBox=\"0 0 16 16\" width=\"16\" xmlns=\"http://www.w3.org/2000/svg\"><desc id=\"__bolt-status-104-desc\">Failed 15m 19s</desc><circle cx=\"8\" cy=\"8\" r=\"8\"></circle><path d=\"M10.984 5.004a.9.9 0 0 1 0 1.272L9.27 7.99l1.74 1.741a.9.9 0 1 1-1.272 1.273l-1.74-1.741-1.742 1.74a.9.9 0 1 1-1.272-1.272l1.74-1.74-1.713-1.714a.9.9 0 0 1 1.273-1.273l1.713 1.713 1.714-1.713a.9.9 0 0 1 1.273 0z\" fill=\"#fff\"></path></svg>";

            Evaluation.GroupEvaluation groupEvaluation = new Evaluation.GroupEvaluation(this);

            double groupScore = groupEvaluation.TotalScore(out double maxGroupScore, out bool groupPasses);
            string groupStatus = groupPasses ? okPath : errorPath;
            string status;
            string output = $"<div class=\"groupStats\">";
            output += $"<h2>{groupStatus} {Team} ({Utils.DoubleToString(groupScore, 2)}/{Utils.DoubleToString(maxGroupScore, 2)})</h2>";

            foreach (string imageFile in Images)
            {
                output += $"<div class=\"reportImage\"><img width=\"350\" src=\"{imageFile}\"/></div>";
            }

            foreach (string unknownAuthor in UnknowUsers)
            { 
                output += $"<div class=\"reportSubItem\">Unknown commit author: {unknownAuthor}</div>";
            }

            foreach (Evaluation.EvaluationItem item in groupEvaluation.EvaluationItems)
            {
                status = item.Pass() ? okPath : errorPath;
                output += $"<div class=\"reportItem\">{status} {item.Name}: {item.Value()}";
                if (item.MinimumValue != null)
                    output += $" (min. {item.MinimumValue})";
                output += "</div>";
                if (item.Score() > 0 && item.MaxScore > 0)
                    output += $"<div class=\"reportSubItem\">Score: {Utils.DoubleToString(item.Score(), 2)}/{Utils.DoubleToString(item.MaxScore, 2)}</div>";

                Evaluation.IEvaluationSubItem subItem = item.SubItems;
                if (subItem != null)
                    output += subItem.Html(Group);
            }

            bool isLeader = true;
            foreach (Student student in Group.Members)
            {
                string user = student.Alias;
                string userStatsDivId = $"userStats-{user}";

                Evaluation.IndividualEvaluation evaluation = new Evaluation.IndividualEvaluation(student.Alias, this, isLeader);
                isLeader = false;//only the first student is the leader

                double individualScore = evaluation.TotalScore(out double maxIndividualScore, out bool memberPasses);
                
                string userStatus = memberPasses & groupPasses ? okPath : errorPath;

                if (true)//(individualScore > 0)
                {
                    output += ToggleSwitch($"{userStatus} {user}: {Utils.DoubleToString(groupScore, 2)}/{Utils.DoubleToString(maxGroupScore, 2)} + " +
                        $"{Utils.DoubleToString(individualScore, 2)}/{Utils.DoubleToString(maxIndividualScore, 2)} = " +
                        $"{Utils.DoubleToString(groupScore + individualScore, 2)}",
                        "reportHeader", userStatsDivId);
                    output += $"<div class=\"userStats\" id=\"{userStatsDivId}\" style=\"display:none\">";
                    foreach (Evaluation.EvaluationItem item in evaluation.EvaluationItems)
                    {
                        if (item.MinimumValue != null)
                            status = item.Pass() ? okPath : errorPath;
                        else
                            status = null;

                        output += $"<div class=\"reportItem\">{status} {item.Name}: {item.Value()}</div>";

                        string settingsText = $"Start: {item.Start.Year}/{item.Start.Month}/{item.Start.Day}";

                        if (item.MinimumValue != null)
                            settingsText += $", Minimum: {item.MinimumValue}";

                        output += $"<div class=\"reportSubItem\">{settingsText}</div>";
                       
                        if (item.MaxScore > 0)
                            output += $"<div class=\"reportSubItem\">Score: +{Utils.DoubleToString(item.Score(), 2)} (/{Utils.DoubleToString(item.MaxScore, 2)})</div>";

                        Evaluation.IEvaluationSubItem subItem = item.SubItems;
                        if (subItem != null)
                            output += subItem.Html(Group);
                    }
                    output += "</div>"; //userStats
                }
                else
                    output += $"<div class=\"reportHeader\">{errorPath} {user}: 0 (no commits)</div>";
            }
            output += "</div>"; //groupStats
            
            return output;
        }

        public static string Serialize(Report report)
        {
            XmlSerializer serializer = new XmlSerializer(report.GetType());

            string output = null;
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, report);
                output = stringWriter.ToString();
            }

            return output;
        }

        public Report Clone()
        {
            return Deserialize<Report>(Serialize(this));
        }

        public static T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            T deserialized;
            using (StringReader streamReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(streamReader))
                {
                    deserialized = (T)serializer.Deserialize(xmlReader);
                }
            }
            return deserialized;
        }
    }
}
