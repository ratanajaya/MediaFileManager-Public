using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetadataEditor;

[SupportedOSPlatform("windows")]
public partial class FormTags : Form
{
    List<Button> tagButtons;

    int colCount = 3;
    int buttonHeight = 30;
    int buttonWidth = 110;
    Color defaultColor = SystemColors.Control;
    Color selectedColor = Color.Aqua;
    FormMain formMain;

    public FormTags(FormMain formMain, List<string> allTags, List<string> selectedTags) {
        InitializeComponent();

        this.formMain = formMain;

        ReziseWindowToWrapItems(allTags);
        DrawButtonFromTags(allTags, selectedTags);
        RefreshSelectedTagButtons(selectedTags);
    }

    void ReziseWindowToWrapItems(List<string> allTags) {
        int rowCount = (int)Math.Ceiling((float)allTags.Count / (float)colCount);
        int height = buttonHeight * rowCount;
        int width = buttonWidth * colCount;

        int heightPadding = 2 * (rowCount + 1);
        int widthPadding = 4 * (colCount + 1); //each button has 4 pixels margin

        this.Height = height + heightPadding;
        this.Width = width + widthPadding;
    }

    void DrawButtonFromTags(List<string> allTags, List<string> selectedTags) {
        tagButtons = new List<Button>();
        int row = -1;
        int col = -1;
        for(int i = 0; i < allTags.Count; i++) {
            Button button = new Button();
            if(i % colCount == 0) {
                row++;
                col = -1;
            }
            col++;

            button.Height = buttonHeight;
            button.Width = buttonWidth;
            button.Location = new Point(col * buttonWidth, row * buttonHeight);
            button.Text = allTags[i];
            button.Font = new Font("Calibri", 11.25f, FontStyle.Regular);
            button.Click += ButtonTags_Click;

            tagButtons.Add(button);
            this.Controls.Add(button);
        }
    }

    void RefreshSelectedTagButtons(List<string> selectedTags) {
        foreach(var button in tagButtons) {
            button.BackColor = selectedTags.Contains(button.Text) ? selectedColor : defaultColor;
        }
    }

    private void ButtonTags_Click(object sender, EventArgs e) {
        var selectedTags = formMain.ChangeSelectedTags(((Button)sender).Text);
        RefreshSelectedTagButtons(selectedTags);
    }

    private void FormTags_Deactivate(object sender, EventArgs e) {
        foreach(var button in tagButtons) {
            button.Click -= ButtonTags_Click;
        }
        this.Close();
    }
}