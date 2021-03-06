﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using collnotes.Data;
using collnotes.Interfaces;

namespace collnotes
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public async void NewProject_OnClick(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProjectPage());
        }

        public async void Collect_OnClick(object sender, EventArgs e)
        {
            try
            {
                List<Project> projectList = DataFunctions.GetProjects();

                string[] projects = new string[projectList.Count + 1];
                for (int i = 0; i < projects.Length - 1; i++)
                {
                    projects[i] = projectList[i].ProjectName;
                }

                // default 'today' project
                Project todayProject = new Project
                {
                    ProjectName = string.Format("Project-{0}", DateTime.Now.ToString("MM-dd-yyyy")),
                    PrimaryCollector = (AppVariables.CollectorName is null) ? "" : AppVariables.CollectorName,
                    CreatedDate = DateTime.Now
                };

                bool dayPExists = false;

                foreach (var p in projectList)
                {
                    if (p.ProjectName.Equals(todayProject.ProjectName))
                    {
                        dayPExists = true;
                        break;
                    }
                }

                if (!dayPExists)
                {
                    projectList.Add(todayProject);
                    projects[projects.Length - 1] = string.Format("Project-{0}", DateTime.Now.ToString("MM-dd-yyyy"));
                }
                else
                {
                    projects = (from p in projectList
                                select p.ProjectName).ToArray();
                }

                var action = await DisplayActionSheet("Choose a project", "Cancel", null, projects);

                foreach (Project p in projectList)
                {
                    if (p.ProjectName.Equals(action))
                    {
                        if (action.Equals(string.Format("Project-{0}", DateTime.Now.ToString("MM-dd-yyyy"))))
                        { // add today project to database if it is selected
                            if (!DataFunctions.CheckExists(p, p.ProjectName))
                            {
                                int autoKeyResult = DataFunctions.InsertObject(p);
                                await Navigation.PushAsync(new CollectingPage(p));
                                break;
                            }
                            else
                            {
                                await Navigation.PushAsync(new CollectingPage(p));
                                break;
                            }
                        }
                        else
                        {
                            await Navigation.PushAsync(new CollectingPage(p));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                DependencyService.Get<ICrossPlatformToast>().ShortAlert("Can't start collecting until you've created a project!");
            }
        }

        public async void ExportProject_OnClick(object sender, EventArgs e)
        {
            try
            {
                List<Project> projectList = DataFunctions.GetProjects();

                if (projectList.Count > 0)
                {
                    await Navigation.PushAsync(new ExportProject());
                }
                else
                {
                    DependencyService.Get<ICrossPlatformToast>().ShortAlert("No projects to export!");
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<ICrossPlatformToast>().ShortAlert("No projects to export!");
                Debug.WriteLine(ex.Message);
            }
        }

        public async void Settings_OnClick(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
