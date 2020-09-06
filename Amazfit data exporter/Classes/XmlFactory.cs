using System;
using System.Data;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using static Amazfit_data_exporter.Classes.Messenger;
using static Amazfit_data_exporter.Classes.Tools;

namespace Amazfit_data_exporter.Classes {
	public class XmlFactory {
		private readonly XDocument _xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
		private readonly XElement _trainingCenterDatabase;

		public XmlFactory() {
			//create header for xml document
			XNamespace aw = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";
			_trainingCenterDatabase = new XElement(aw + "TrainingCenterDatabase");
			var xsi = new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance");
			_trainingCenterDatabase.Add(xsi);
		}

		public void createXmlFile(DataRow summary, DataTable workoutInfo) {
			//TODO messy code from old version of exporter - needs rewrite code
			//get Json data from summary table (content column)
			dynamic summaryInfo = JObject.Parse((string) summary["content"]);
			//get pause info into array
			JArray pauses = summaryInfo["pause_info"];

			var startTime = dateConvertor((long) summary["start_time"]);
			//create lap
			var lap = new XElement(
				"Lap",
				new XAttribute("StartTime",
							   startTime.ToString("yyyy-MM-dd") + "T" + startTime.ToString("HH:mm:ss") + "Z"));
			lap.SetElementValue("TotalTimeSeconds", ((long) summary["end_time"] - (long) summary["start_time"]) / 1000);
			lap.SetElementValue("Intensity", "Active");

			//create array of tracks relative to the number of pauses
			var tracks = new XElement[pauses.Count + 1];
			for (var i = 0; i < tracks.Length; i++)
				tracks[i] = new XElement("Track");
			
			//create trackpoints inside tracks
            foreach (DataRow entry in workoutInfo.Rows)
            {
                var time = (long) entry["time"];
                var dateTime = dateConvertor(time);
                var heart = (double) entry["rate"];

                //compose trackpoint element
                var trackpoint = new XElement("Trackpoint");
                trackpoint.SetElementValue("Time", dateTime.ToString("yyyy-MM-dd") + "T" + dateTime.ToString("HH:mm:ss") + "Z");
                var heartRateBpm = new XElement("HeartRateBpm");
                heartRateBpm.SetElementValue("Value", heart);
                trackpoint.Add(heartRateBpm);

                //find out, where put this trackpoint into
                for (var i = 0; i < tracks.Length; i++)
                {
                    //is trackpoint assigned?
                    var assigned = false;

                    //if there is only one track
                    if (tracks.Length == 1)
                    {
                        tracks[0].Add(trackpoint);
                        assigned = true;
                    }

                    //if there is more tracks - decide, where put trackpoint
                    if (i == 0 && !assigned)
                    {
                        //first
                        if (time <= Convert.ToInt64(pauses[i]["start_time"].ToString()))
                        {
                            tracks[i].Add(trackpoint);
                            assigned = true;
                        }
                    }
                    else if (i == tracks.Length - 1 && !assigned)
                    {
                        //last
                        if (time >= Convert.ToInt64(pauses[i - 1]["end_time"].ToString()))
                        {
                            tracks[i].Add(trackpoint);
                            assigned = true;
                        }
                    }
                    else if (!assigned && i != 0 && i != tracks.Length - 1)
                    {
                        //somewhere between first and last
                        if (time >= Convert.ToInt64(pauses[i - 1]["end_time"].ToString()) && time <= Convert.ToInt64(pauses[i]["start_time"].ToString()))
                        {
                            tracks[i].Add(trackpoint);
                            assigned = true;
                        }
                    }
                }
            }
			
			//add all tracks to Lap
			foreach (var track in tracks)
			{
				lap.Add(track);
			}

			var sportName = Tools.sportName((long) summary["type"]);
			if (sportName == "")
			{
				sportName = "Other";
			}

			var activities = new XElement("Activities");
			var activity = new XElement("Activity", 
											 new XAttribute("Sport", sportName)
			);
			activity.SetElementValue("Id", startTime.ToString("yyyy-MM-dd") + "T" + startTime.ToString("HH:mm:ss") + "Z");
			activity.Add(lap);
			activities.Add(activity);
			_trainingCenterDatabase.Add(activities);
			_xDoc.Add(_trainingCenterDatabase);

			saveXDoc(_xDoc, sportName, startTime);
		}

		private static void saveXDoc(XDocument doc, string sportName, DateTime startTime) {
			if (sportName == "" || sportName == "Other")
				sportName = "Unknown";
			var startTimeString = startTime.ToString("yyyy_MM_dd HH_mm");
			doc.Save(Paths.workoutDateFolderFilePath(sportName, startTimeString).cleanPath());
			doc.Save(Paths.workoutSportNameFolderFilePath(sportName, startTimeString).cleanPath());
			doc.Save(Paths.workoutLastExportFolderFilePath(sportName, startTimeString).cleanPath());


			sendMessage(startTimeString + " -" + sportName + "- ", DefaultMsg, false);
			if (File.Exists(Paths.workoutDateFolderFilePath(sportName, startTimeString).cleanPath()))
				sendMessage("Successfully exported", SuccessMsg);
			else
				sendMessage("Error - file is not exported!", ErrorMsg);
		}
	}
}