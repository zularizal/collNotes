﻿using System;
using System.Diagnostics;
using Xamarin.Forms;
using collnotes.Data;
using collnotes.Interfaces;
using collnotes.Plugins;

namespace collnotes
{
    /// <summary>
    /// Specimen page.
    /// </summary>
    public partial class SpecimenPage : ContentPage
    {
        // object instance(s) for current Specimen
        private Site site;
        private Specimen specimen;

        // flags used to control what happens in some events
        private bool userIsEditing = false;
        private bool editWasSaved = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:collnotes.SpecimenPage"/> class.
        /// Empty constructor, is only used by the Device Preview feature of Visual Studio.
        /// </summary>
        public SpecimenPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:collnotes.SpecimenPage"/> class.
        /// Create Constructor. Takes the Site to add a Specimen under as an argument.
        /// </summary>
        /// <param name="site">Site.</param>
        public SpecimenPage(Site site)
        {
            specimen = new Specimen();
            this.site = site;
            InitializeComponent();
            Title = (AppVariables.CollectionCount > 0) ? site.RecordNo.ToString() + "-" + (AppVariables.CollectionCount + 1).ToString() : Title = site.RecordNo.ToString() + "-1";
            btnBack.IsVisible = userIsEditing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:collnotes.SpecimenPage"/> class.
        /// Edit Constructor. Takes the Specimen the User wishes to edit as an argument.
        /// </summary>
        /// <param name="specimen">Specimen.</param>
        public SpecimenPage(Specimen specimen)
        {
            this.specimen = specimen;
            site = DataFunctions.GetSiteByName(specimen.SiteName);
            InitializeComponent();

            userIsEditing = true;
            editWasSaved = false;

            Title = site.RecordNo + "-" + specimen.SpecimenNumber;
            entryFieldID.Text = specimen.FieldIdentification;
            entryFieldID.IsEnabled = false;
            entryOccurrenceNotes.Text = specimen.OccurrenceNotes;
            entrySubstrate.Text = specimen.Substrate;
            pickerLifeStage.SelectedItem = specimen.LifeStage;
            switchCultivated.IsToggled = specimen.Cultivated;
            entryIndivCount.Text = specimen.IndividualCount;
            btnNewSpecimen.IsEnabled = false;

            btnBack.IsVisible = userIsEditing;
        }

        /// <summary>
        /// pickerLifeStage SelectedIndexChange event.
        /// Sets the current Specimen LifeStage property based on the User's selection.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void pickerLifeStage_SelectedIndexChange(object sender, EventArgs e)
        {
            if (pickerLifeStage.SelectedItem == null)
            {
                return;
            }
            specimen.LifeStage = pickerLifeStage.SelectedItem.ToString();
        }

        /// <summary>
        /// switchCultivated Toggled event.
        /// Sets the current Specimen Cultivated parameter based on the IsToggled property of this switch.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void switchCultivated_Toggled(object sender, EventArgs e)
        {
            specimen.Cultivated = switchCultivated.IsToggled;
        }

        /// <summary>
        /// btnSetSpecimenPhoto Click event.
        /// Allows the User to take a photo that is saved with the filename site#-specimen#.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public async void btnSetSpecimenPhoto_Clicked(object sender, EventArgs e)
        {
            await TakePhoto.CallCamera(site.RecordNo.ToString() + "-" + specimen.SpecimenNumber.ToString());
        }

        /// <summary>
        /// btnSaveSpecimen Click event.
        /// Saves or Updates the current Specimen.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void btnSaveSpecimen_Clicked(object sender, EventArgs e)
        {
            if (userIsEditing)
            {
                specimen.Substrate = entrySubstrate.Text;
                specimen.IndividualCount = entryIndivCount.Text;
                specimen.Cultivated = switchCultivated.IsToggled;
                specimen.OccurrenceNotes = entryOccurrenceNotes.Text;
                specimen.LifeStage = pickerLifeStage.SelectedItem.ToString();
                specimen.GPSCoordinates = site.GPSCoordinates;

                int updateResult = DatabaseFile.GetConnection().Update(specimen, typeof(Specimen));
                if (updateResult == 1)
                {
                    DependencyService.Get<ICrossPlatformToast>().ShortAlert(specimen.SpecimenName + " save succeeded.");
                    return;
                }
                else
                {
                    DependencyService.Get<ICrossPlatformToast>().ShortAlert(specimen.SpecimenName + " save failed");
                    return;
                }
            }

            SaveCurrentSpecimen();

            DependencyService.Get<ICrossPlatformToast>().ShortAlert("Saved " + specimen.SpecimenName);
            btnNewSpecimen_Clicked(this, e); // we could continue here or force the user to go back to the CollectingPage, need more user input...
        }

        /// <summary>
        /// btnNewSpecimen Click event.
        /// Clears the Page for a new Specimen.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void btnNewSpecimen_Clicked(object sender, EventArgs e)
        {
            specimen = new Specimen();

            entryFieldID.Text = "";
            entryOccurrenceNotes.Text = "";
            entrySubstrate.Text = "";
            pickerLifeStage.SelectedItem = null;
            switchCultivated.IsToggled = false;
            entryIndivCount.Text = "";

            lblStatusMessage.IsVisible = false;
            lblStatusMessage.Text = "";

            specimen.SiteName = site.SiteName;

            if (DataFunctions.CheckExists(specimen, site.RecordNo.ToString() + "-" + AppVariables.CollectionCount.ToString()))
            {
                Title = site.RecordNo.ToString() + "-" + (AppVariables.CollectionCount + 1).ToString();
            }
            else
            {
                Title = site.RecordNo.ToString() + "-" + AppVariables.CollectionCount.ToString();
            }
        }

        /// <summary>
        /// btnBack Click event.
        /// Provides the user with the option to exit an Update page.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private async void btnBack_Clicked(object sender, EventArgs e)
        {
            if (editWasSaved)
            {
                Navigation.RemovePage(this);
                return;
            }

            bool response = await DisplayAlert("Are you sure?", "Are you sure you don't want to save your changes?", "Yes", "No");
            if (response)
                Navigation.RemovePage(this);
        }

        /// <summary>
        /// Saves the current specimen.
        /// </summary>
        /// <returns><c>true</c>, if current specimen was saved, <c>false</c> otherwise.</returns>
        private bool SaveCurrentSpecimen()
        {
            specimen.SiteName = site.SiteName;
            specimen.GPSCoordinates = site.GPSCoordinates;
            specimen.OccurrenceNotes = entryOccurrenceNotes.Text is null ? "" : entryOccurrenceNotes.Text;
            specimen.Substrate = entrySubstrate.Text is null ? "" : entrySubstrate.Text;
            specimen.IndividualCount = entryIndivCount.Text is null ? "" : entryIndivCount.Text;
            specimen.Cultivated = switchCultivated.IsToggled;

            AppVariables.CollectionCount = AppVariables.CollectionCount > 0 ? AppVariables.CollectionCount + 1 : 1;

            specimen.SpecimenNumber = AppVariables.CollectionCount;

            specimen.FieldIdentification = entryFieldID.Text is null ? "" : entryFieldID.Text;

            specimen.SpecimenName = "Specimen-" + specimen.SpecimenNumber;

            specimen.LifeStage = pickerLifeStage.SelectedItem is null ? "" : pickerLifeStage.SelectedItem.ToString();

            int autoKeyResult = DatabaseFile.GetConnection().Insert(specimen);
            Debug.WriteLine("inserted specimen, recordno is: " + autoKeyResult.ToString());

            // anytime we add a specimen we need to write back the CollectionCount
            AppVarsFile.WriteAppVars();

            return true;
        }
    }
}
