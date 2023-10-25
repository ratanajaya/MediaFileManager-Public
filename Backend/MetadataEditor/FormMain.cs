using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Drawing.Imaging;
using System.Diagnostics;
using SharedLibrary;
using MetadataEditor.AL.Models;
using MetadataEditor.AL.Services;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using C = SharedLibrary.Constants;

namespace MetadataEditor;

public partial class FormMain : Form
{
    string _currentRootFolder; //for folder picker dialog, filled with the previous path of the selected folder
    int _currentFileIndex = 0; //For album display
    IAppLogic _al;
    IAlbumInfoProvider _ai;
    ConfigurationModel _config;

    AlbumViewModel _viewModel;
    AlbumDeleteStatusEnum _deleteStatus;
    List<FileDisplayModel> _fileDisplays;
    string _cachedFolderNext;
    string _cachedFolderPrev;

    Dictionary<string, string> _shortDisplayMap = new Dictionary<string, string>() {
            { "MG", C.Category.Manga },
            { "CG", C.Category.CGSet },
            { "SC", C.Category.SelfComp },
            { "Ptr", C.Orientation.Portrait },
            { "Lsc", C.Orientation.Landscape },
            { "Aut", C.Orientation.Auto },
            { "EN", C.Language.English },
            { "JP", C.Language.Japanese },
            { "CH", C.Language.Chinese },
            { "Other", C.Language.Other }
        };

    #region Initialization
    public FormMain(IAppLogic appLogic, IAlbumInfoProvider ai, ConfigurationModel config) {
        InitializeComponent();

        _al = appLogic;
        _ai = ai;
        _config = config;

        lbPage.Parent = pctCover;
        pctCover.MouseWheel += PctCover_MouseWheel;
        pctCover.PreviewKeyDown += AnyControlPreviewKeyDown;
        _deleteStatus = AlbumDeleteStatusEnum.NotAllowed;

        groupAlbum.AllowDrop = true;
    }
        
    private void FormMain_Load(object sender, EventArgs e) {
        txtTags.Text = "";
        InitializeEmptyAlbumViewModel();
        if(_config.Args.Any()) {
            var path = _config.Args[0];
            var viewModel = Task.Run(() => _al.GetAlbumViewModelAsync(path, _viewModel?.Album)).GetAwaiter().GetResult();
            AssignViewModel(viewModel);
        }
    }

    private void InitializeEmptyAlbumViewModel() {
        _viewModel = new AlbumViewModel {
            Album = new Album()
        };
    }
    #endregion

    #region Browse folder for album
    private async void BtnBrowse_Click(object sender, EventArgs e) {
        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = _currentRootFolder ?? _config.BrowsePath;
        dialog.IsFolderPicker = true;
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            await AssignAlbumPath(dialog.FileName);
        }
    }

    private async Task AssignAlbumPath(string path) {
        var viewModel = await _al.GetAlbumViewModelAsync(path, _viewModel?.Album);
        AssignViewModel(viewModel);
    }

    private void AssignViewModel(AlbumViewModel vm) {
        _viewModel = vm;
        _currentRootFolder = vm.Path.Replace(vm.Path.Split('\\').Last(), "");
        _fileDisplays = GetFileDisplayModels(vm.Path, vm.AlbumFiles);
        DisplayAlbum();
    }

    List<FileDisplayModel> GetFileDisplayModels(string albumDir, List<string> filePaths) {
        return filePaths.Select(path => new FileDisplayModel {
            Path = path,
            FileNameDisplay = Path.GetFileName(path),
            SubDirDisplay = Path.GetDirectoryName(path).Replace(albumDir, ""),
            UploadStatus = "_"
        }).ToList();
    }

    private void FormMain_DragEnter(object sender, DragEventArgs e) {
        if(e.Data.GetDataPresent(DataFormats.FileDrop) && Directory.Exists(((string[])e.Data.GetData(DataFormats.FileDrop))[0]))
            e.Effect = DragDropEffects.Copy;
    }

    private async void FormMain_DragDrop(object sender, DragEventArgs e) {
        if(!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
        if(!Directory.Exists(path)) return;

        await AssignAlbumPath(path);
    }
    #endregion

    #region Display Album Infos
    void DisplayAlbum() {
        ClearControls();

        txtTitle.Text = _viewModel.Album.Title;
        txtArtists.Text = String.Join(",", _viewModel.Album.Artists);
        txtTags.Text = _viewModel.Album.GetTagsDisplay();
        txtCharacters.Text = _viewModel.Album.GetCharactersDisplay();
        txtNote.Text = _viewModel.Album.Note ?? "";

        foreach (string lan in _viewModel.Album.Languages) {
            string controlName = "chkLan" + lan;
            ((CheckBox)this.Controls.Find(controlName, true)[0]).Checked = true;
        }
        SetRadioButton("rbCat", _viewModel.Album.Category);
        txtTier.Text = _viewModel.Album.Tier.ToString();
        SetRadioButton("rbOri", _viewModel.Album.Orientation);
        chkIsWipTrue.Checked = _viewModel.Album.IsWip;
        chkIsReadTrue.Checked = _viewModel.Album.IsRead;
        txtFolder.Text = Path.GetFileName(_viewModel.Path);

        UpdateFileDisplay();

        _currentFileIndex = 0;

        var extension = Path.GetExtension(_viewModel.AlbumFiles[_currentFileIndex]);

        if(_ai.SuitableImageFormats.Contains(extension) && extension != ".webp") {
            //2022-12-29 I found out that webp can be animated. Causing crash if loaded
            using(var fs = File.Open(_viewModel.AlbumFiles[_currentFileIndex], FileMode.Open, FileAccess.Read, FileShare.Read)) {
                pctCover.Image = Image.FromStream(fs);
            }
            pctCover.SizeMode = PictureBoxSizeMode.Zoom;
        }
        else {
            pctCover.Image = null;
        }

        lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;

        SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum.NotAllowed);
        _cachedFolderNext = null;
        _cachedFolderPrev = null;
    }

    void UpdateFileDisplay() {
        string[] lines = _fileDisplays.Select(f => f.SubDirDisplay + "|" + f.FileNameDisplay + "|" + f.UploadStatus).ToArray();

        txtFileUpload.Lines = lines;
        txtFileUpload.SelectionStart = txtFileUpload.Text.Length;
        txtFileUpload.ScrollToCaret();
    }

    void SetRadioButton(string v, string category) {
        RadioButton rb = (RadioButton)this.Controls[0].Controls[v].Controls[v + category];
        rb.Select();
    }

    void ClearControls() {
        foreach (Control control in this.Controls[0].Controls) {
            if (control is TextBox) {
                TextBox txtbox = (TextBox)control;
                txtbox.Text = string.Empty;
            }
            else if (control is ListView) {
                ((ListView)control).Clear();
            }
            else if (control is CheckBox) {
                CheckBox chkbox = (CheckBox)control;
                chkbox.Checked = false;
            }
            else if (control is RadioButton) {
                RadioButton rdbtn = (RadioButton)control;
                rdbtn.Checked = false;
            }
            else if (control is DateTimePicker) {
                DateTimePicker dtp = (DateTimePicker)control;
                dtp.Value = DateTime.Now;
            }
        }
    }
    #endregion

    #region Save #album.json
    private async void BtnCreate_Click(object sender, EventArgs e) {
        RetrieveAlbumVmValueFromUI();
        string retval = await _al.SaveAlbumJson(_viewModel);
    }

    private void RetrieveAlbumVmValueFromUI() {
        _viewModel.Album.Title = txtTitle.Text;
        _viewModel.Album.Artists = txtArtists.Text.Split(',').ToList();
        _viewModel.Album.Category = GetFromRadioButton("rbCat");
        _viewModel.Album.Tier = int.Parse(txtTier.Text);
        _viewModel.Album.Orientation = GetFromRadioButton("rbOri");
        _viewModel.Album.Tags = txtTags.Text.Split(',').Select(x => x.Trim()).ToList();
        _viewModel.Album.Characters = txtCharacters.Text.Split(',').Select(x => x.Trim()).ToList();
        _viewModel.Album.Note = txtNote.Text;
        _viewModel.Album.Languages = GetLansFromForm();
        _viewModel.Album.IsWip = chkIsWipTrue.Checked;
        _viewModel.Album.IsRead = chkIsReadTrue.Checked;

        _viewModel.Album.ValidateAndCleanup();
    }

    private string GetFromRadioButton(string v) {
        foreach (Control c in this.Controls[0].Controls[v].Controls) {
            if (((RadioButton)c).Checked) {
                return _shortDisplayMap[c.Text];
            }
        }
        return "";
    }

    List<string> GetLansFromForm() {
        List<string> result = new List<string>();

        foreach (Control c in this.Controls[0].Controls) {
            if (c.Name.Contains("chkLan")) {
                if (((CheckBox)c).Checked) {
                    result.Add(_shortDisplayMap[c.Text]);
                }
            }
        }

        return result;
    }
    #endregion

    #region Next/Prev
    string GetRelativeFolder(string currentFolder, int step) {
        string rootFolder = Path.GetDirectoryName(currentFolder);
        string[] allFolders = Directory.GetDirectories(rootFolder).OrderByAlphaNumeric(a => a).ToArray();

        int relativeFolderIndex = (Array.IndexOf(allFolders,currentFolder) + step) % allFolders.Length;
        string result = allFolders[relativeFolderIndex];
        return result;
    }

    private async void BtnNext_Click(object sender, EventArgs e) {
        try {
            string fullPath = string.IsNullOrEmpty(_cachedFolderNext) ? GetRelativeFolder(_viewModel.Path, 1) : _cachedFolderNext;
            _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
            _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel?.Album);
            _fileDisplays = GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles);
            DisplayAlbum();
        }
        catch(Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    private async void BtnPrev_Click(object sender, EventArgs e) {
        try {
            string fullPath = string.IsNullOrEmpty(_cachedFolderPrev) ? GetRelativeFolder(_viewModel.Path, -1) : _cachedFolderPrev;
            _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
            _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel?.Album);
            _fileDisplays = GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles);
            DisplayAlbum();
        }
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    private void BtnPrevPage_Click(object sender, EventArgs e) {
        //PrevPage();
    }

    private void BtnNextPage_Click(object sender, EventArgs e) {
        //NextPage();
    }

    private void PctCover_Hover(object sender, EventArgs e) {
        pctCover.Focus();
    }

    private void PctCover_MouseWheel(object sender, MouseEventArgs e) {
        if(e.Delta > 0) {
            MovePage(-1);
        }
        else {
            MovePage(1);
        }
    }

    private void AnyControlPreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
        if(e.KeyCode == Keys.A) {
            MovePage(-1);
        }
        else if(e.KeyCode == Keys.D) {
            MovePage(1);
        }
    }

    void MovePage(int step) {
        if(_viewModel.AlbumFiles == null) { return; }
        try {
            _currentFileIndex = _currentFileIndex + step;
            if(_currentFileIndex < 0) {
                _currentFileIndex = _viewModel.AlbumFiles.Count - 1;
            }
            else {
                _currentFileIndex = _currentFileIndex % _viewModel.AlbumFiles.Count;
            }

            using(var bm = new Bitmap(_viewModel.AlbumFiles[_currentFileIndex])) {
                pctCover.Image = new Bitmap(bm);
            }
            lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;
        }
        catch (Exception ex) {
            pctCover.Image = null;
            lbPage.Text = "";

            Console.WriteLine(ex.ToString());
        }
    }
    #endregion

    #region Other UI Actions
    private void btnTierMin_Click(object sender, EventArgs e) {
        int res;
        int val = int.TryParse(txtTier.Text, out res) ? res : 0;
        txtTier.Text = (val - 1).ToString();
    }

    private void btnTierPlus_Click(object sender, EventArgs e) {
        int res;
        int val = int.TryParse(txtTier.Text, out res) ? res : 0;
        txtTier.Text = (val + 1).ToString();
        chkIsReadTrue.Checked = true;
    }

    public List<string> ChangeSelectedTags(string tag) {
        txtTags.Text = txtTags.Text.Trim();
        List<string> tags = txtTags.Text.Split(new[] { "," }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
        tags.Remove("");
        if(tags.Contains(tag)) { //Remove if exist
            tags.Remove(tag);
        }
        else { //Add if not exist
            tags.Add(tag);
        }
        tags.Sort();
        txtTags.Text = string.Join(", ", tags);

        return tags;
    }

    public List<string> ChangeSelectedCharacters(string character) {
        txtCharacters.Text = txtCharacters.Text.Trim();
        List<string> characters = txtCharacters.Text.Split(new[] { "," }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
        characters.Remove("");
        if(characters.Contains(character)) { //Remove if exist
            characters.Remove(character);
        }
        else { //Add if not exist
            characters.Add(character);
        }
        characters.Sort();
        txtCharacters.Text = string.Join(", ", characters);

        return characters;
    }

    private void btnHp_Click(object sender, EventArgs e) {
        txtNote.Text = "HP";
    }

    private void CbTags_SelectedIndexChanged(object sender, EventArgs e) {
        //cbTags.DroppedDown = true;

        //ChangeSelectedTags(cbTags.SelectedItem.ToString());
    }

    private void btnPopupTags_Click(object sender, EventArgs e) {
        FormTags formTags = new FormTags(this, _al.GetTags().ToList(), _viewModel.Album.Tags);
        formTags.StartPosition = FormStartPosition.Manual;
        formTags.ShowInTaskbar = false;
        formTags.ShowIcon = false;
        formTags.ControlBox = false;
        formTags.Text = String.Empty;

        var location = this.Location;
        var marginLeft = btnPopupTags.Left + btnPopupTags.Width;
        var marginTop = btnPopupTags.Top + btnPopupTags.Height;
        location.Offset(marginLeft, marginTop);
        formTags.Location = location;
        formTags.Show(btnPopupTags);
    }

    private void btnPopupCharacters_Click(object sender, EventArgs e) {
        FormCharacters formCharacters = new FormCharacters(this, _al.GetCharacters().ToList(), _viewModel.Album.Characters);
        formCharacters.StartPosition = FormStartPosition.Manual;
        formCharacters.ShowInTaskbar = false;
        formCharacters.ShowIcon = false;
        formCharacters.ControlBox = false;
        formCharacters.Text = String.Empty;

        var location = this.Location;
        var marginLeft = btnPopupCharacters.Left + btnPopupCharacters.Width;
        var marginTop = btnPopupCharacters.Top + btnPopupCharacters.Height;
        location.Offset(marginLeft, marginTop);
        formCharacters.Location = location;
        formCharacters.Show(btnPopupCharacters);
    }

    private void btnExplore_Click(object sender, EventArgs e) {
        try {
            string fileName = _viewModel.Path;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "explorer.exe";
            startInfo.Arguments = "\"" + fileName + "\"";
            Process.Start(startInfo);
        }
        catch(Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    private async void btnPost_Click(object sender, EventArgs e) {
        if(_viewModel.Path == null) { return; }
        RetrieveAlbumVmValueFromUI();

        var saveTask = _al.SaveAlbumJson(_viewModel);

        var progress = new Progress<FileDisplayModel>(model => {
            var uploadedFIle = _fileDisplays.FirstOrDefault(fd => fd.Path == model.Path);
            uploadedFIle.UploadStatus = model.UploadStatus;

            UpdateFileDisplay();
        });

        var albumId = await _al.PostAlbumJsonOffline(_viewModel, progress);
        var saveTaskRetval = await saveTask;

        SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum.Allowed);
    }

    private async void btnPostMetadata_Click(object sender, EventArgs e) {
        if(_viewModel.Path == null) { return; }

        RetrieveAlbumVmValueFromUI();
        await _al.SaveAlbumJson(_viewModel);
        var albumId = _al.PostAlbumMetadataOffline(_viewModel);
        var confirmResult = MessageBox.Show(albumId, "Success", MessageBoxButtons.OK);
    }

    private void btnDelete_Click(object sender, EventArgs e) {
        try {
            _cachedFolderNext = GetRelativeFolder(_viewModel.Path, 1);
            _cachedFolderPrev = GetRelativeFolder(_viewModel.Path, -1);

            Directory.Delete(_viewModel.Path, true);

            SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum.Deleted);
        }
        catch(Exception ex) {
            var confirmResult = MessageBox.Show(ex.Message,"Exception", MessageBoxButtons.OK);
        }
    }

    private void SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum deleteStatus) {
        _deleteStatus = deleteStatus;
        bool enableUIGroup = true;

        if(deleteStatus == AlbumDeleteStatusEnum.NotAllowed) {
            enableUIGroup = true;
            btnDelete.Enabled = false;
        }
        else if(deleteStatus == AlbumDeleteStatusEnum.Allowed) {
            enableUIGroup = true;
            btnDelete.Enabled = true;
        }
        else if(deleteStatus == AlbumDeleteStatusEnum.Deleted) {
            enableUIGroup = false;
        }

        btnPost.Enabled = enableUIGroup;
        btnCreate.Enabled = enableUIGroup;
    }

    private void btnSetName_Click(object sender, EventArgs e) {
        if(string.IsNullOrWhiteSpace(_viewModel.Path) || string.IsNullOrWhiteSpace(txtFolder.Text)) return;

        var newFullPath = Path.Combine(Path.GetDirectoryName(_viewModel.Path), txtFolder.Text);
        var tempVm = _al.RenameAlbumPath(_viewModel, newFullPath);
        AssignViewModel(tempVm);
    }
    #endregion
}