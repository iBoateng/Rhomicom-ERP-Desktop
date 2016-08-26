using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EventsAndAttendance.Classes;

namespace EventsAndAttendance.Dialogs
{
  public partial class attchmntsDiag : Form
  {
    public attchmntsDiag()
    {
      InitializeComponent();
    }
    cadmaFunctions.NavFuncs myNav = new cadmaFunctions.NavFuncs();

    private long totl_vals = 0;
    private long cur_vals_idx = 0;
    private string vwSQLStmnt = "";
    private bool is_last_val = false;
    bool obeyEvnts = false;
    long last_vals_num = 0;
    public long batchid = -1;
    public long batchHdrID = -1;

    private void attchmntsDiag_Load(object sender, EventArgs e)
    {
      Color[] clrs = Global.mnFrm.cmCde.getColors();
      this.BackColor = clrs[0];
      this.loadValPanel();
    }


    private void loadValPanel()
    {
      this.obeyEvnts = false;
      if (this.searchInComboBox.SelectedIndex < 0)
      {
        this.searchInComboBox.SelectedIndex = 0;
      }
      int dsply = 0;
      if (this.dsplySizeComboBox.Text == ""
       || int.TryParse(this.dsplySizeComboBox.Text, out dsply) == false)
      {
        this.dsplySizeComboBox.Text = Global.mnFrm.cmCde.get_CurPlcy_Mx_Dsply_Recs().ToString();
      }

      if (this.searchForTextBox.Text == "")
      {
        this.searchForTextBox.Text = "%";
      }
      this.is_last_val = false;
      this.totl_vals = Global.mnFrm.cmCde.Big_Val;
      this.getValPnlData();
      this.obeyEvnts = true;
    }

    private void getValPnlData()
    {
      this.updtValTotals();
      this.populateValGridVw();
      this.updtValNavLabels();
    }

    private void updtValTotals()
    {
      this.myNav.FindNavigationIndices(int.Parse(this.dsplySizeComboBox.Text),
      this.totl_vals);

      if (this.cur_vals_idx >= this.myNav.totalGroups)
      {
        this.cur_vals_idx = this.myNav.totalGroups - 1;
      }
      if (this.cur_vals_idx < 0)
      {
        this.cur_vals_idx = 0;
      }
      this.myNav.currentNavigationIndex = this.cur_vals_idx;
    }

    private void updtValNavLabels()
    {
      this.moveFirstButton.Enabled = this.myNav.moveFirstBtnStatus();
      this.movePreviousButton.Enabled = this.myNav.movePrevBtnStatus();
      this.moveNextButton.Enabled = this.myNav.moveNextBtnStatus();
      this.moveLastButton.Enabled = this.myNav.moveLastBtnStatus();
      this.positionTextBox.Text = this.myNav.displayedRecordsNumbers();
      if (this.is_last_val == true ||
        this.totl_vals != Global.mnFrm.cmCde.Big_Val)
      {
        this.totalRecLabel.Text = this.myNav.totalRecordsLabel();
      }
      else
      {
        this.totalRecLabel.Text = "of Total";
      }
    }

    private void populateValGridVw()
    {
      this.obeyEvnts = false;
      DataSet dtst;
      long hdrID = this.batchid;
      if (hdrID <= 0)
      {
        hdrID = this.batchHdrID;
      }
      dtst = Global.get_Attachments(this.searchForTextBox.Text,
    this.searchInComboBox.Text, this.cur_vals_idx,
    int.Parse(this.dsplySizeComboBox.Text), hdrID, ref this.vwSQLStmnt);

      this.attchmntsListView.Items.Clear();
      for (int i = 0; i < dtst.Tables[0].Rows.Count; i++)
      {
        this.last_vals_num = this.myNav.startIndex() + i;
        ListViewItem nwItem = new ListViewItem(new string[] {
    (this.myNav.startIndex() + i).ToString(),
    dtst.Tables[0].Rows[i][2].ToString(),dtst.Tables[0].Rows[i][3].ToString(),
    dtst.Tables[0].Rows[i][0].ToString()});
        this.attchmntsListView.Items.Add(nwItem);
      }
      this.correctValsNavLbls(dtst);
      if (this.attchmntsListView.Items.Count > 0)
      {
        this.obeyEvnts = true;
        this.attchmntsListView.Items[0].Selected = true;
      }
      this.obeyEvnts = true;
    }

    private void correctValsNavLbls(DataSet dtst)
    {
      long totlRecs = dtst.Tables[0].Rows.Count;
      if (this.cur_vals_idx == 0 && totlRecs == 0)
      {
        this.is_last_val = true;
        this.totl_vals = 0;
        this.last_vals_num = 0;
        this.cur_vals_idx = 0;
        this.updtValTotals();
        this.updtValNavLabels();
      }
      else if (this.totl_vals == Global.mnFrm.cmCde.Big_Val
  && totlRecs < long.Parse(this.dsplySizeComboBox.Text))
      {
        this.totl_vals = this.last_vals_num;
        if (totlRecs == 0)
        {
          this.cur_vals_idx -= 1;
          this.updtValTotals();
          this.populateValGridVw();
        }
        else
        {
          this.updtValTotals();
        }
      }
    }

    private void valPnlNavButtons(object sender, System.EventArgs e)
    {
      System.Windows.Forms.ToolStripButton sentObj =
        (System.Windows.Forms.ToolStripButton)sender;
      this.totalRecLabel.Text = "";
      if (sentObj.Name.ToLower().Contains("first"))
      {
        this.cur_vals_idx = 0;
      }
      else if (sentObj.Name.ToLower().Contains("previous"))
      {
        this.cur_vals_idx -= 1;
      }
      else if (sentObj.Name.ToLower().Contains("next"))
      {
        this.cur_vals_idx += 1;
      }
      else if (sentObj.Name.ToLower().Contains("last"))
      {
        long hdrID = this.batchid;
        if (hdrID <= 0)
        {
          hdrID = this.batchHdrID;
        }

        this.totl_vals = Global.get_Total_Attachments(
          this.searchForTextBox.Text, this.searchInComboBox.Text,
          hdrID);

        this.is_last_val = true;
        this.updtValTotals();
        this.cur_vals_idx = this.myNav.totalGroups - 1;
      }
      this.getValPnlData();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void attchmntsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.attchmntsListView.SelectedItems.Count > 0 && this.obeyEvnts == true)
      {
        Global.mnFrm.cmCde.getDBImageFile(
  this.attchmntsListView.SelectedItems[0].SubItems[2].Text,
  Global.mnFrm.cmCde.getAttnDocsImgsDrctry(), ref this.prvwPictureBox);
      }
    }

    private void gotoButton_Click(object sender, EventArgs e)
    {
      this.loadValPanel();
    }

    private void exptExclTSrchMenuItem_Click(object sender, EventArgs e)
    {
      Global.mnFrm.cmCde.exprtToExcel(this.attchmntsListView);
    }

    private void positionTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      EventArgs ex = new EventArgs();
      if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
      {
        this.valPnlNavButtons(this.movePreviousButton, ex);
      }
      else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
      {
        this.valPnlNavButtons(this.moveNextButton, ex);
      }
    }

    private void searchForTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      EventArgs ex = new EventArgs();
      if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
      {
        this.gotoButton_Click(this.gotoButton, ex);
      }
    }

    private void vwSQLButton_Click(object sender, EventArgs e)
    {
      Global.mnFrm.cmCde.showSQL(this.vwSQLStmnt, 6);
    }

    private void rfrshTsrchMenuItem_Click(object sender, EventArgs e)
    {
      this.gotoButton_Click(this.gotoButton, e);
    }

    private void rcHstryTsrchMenuItem_Click(object sender, EventArgs e)
    {
      if (this.attchmntsListView.SelectedItems.Count <= 0)
      {
        Global.mnFrm.cmCde.showMsg("Please select a Transaction First!", 0);
        return;
      }
      Global.mnFrm.cmCde.showRecHstry(Global.mnFrm.cmCde.get_Gnrl_Rec_Hstry(long.Parse(
        this.attchmntsListView.SelectedItems[0].SubItems[3].Text),
        "accb.accb_batch_trns_attchmnts", "attchmnt_id"), 9);
    }

    private void vwSQLTsrchMenuItem_Click(object sender, EventArgs e)
    {
      this.vwSQLButton_Click(this.vwSQLButton, e);
    }

    private void addButton_Click(object sender, EventArgs e)
    {
      addAttchmntDiag nwDiag = new addAttchmntDiag();
      nwDiag.attchmntIDTextBox.Text = "-1";
      nwDiag.batchID = this.batchid;
      DialogResult dgrs = nwDiag.ShowDialog();
      if (dgrs == DialogResult.OK)
      {
        if (Global.mnFrm.cmCde.myComputer.FileSystem.FileExists(
          nwDiag.fileNmTextBox.Text) == true)
        {
          Global.createAttachment(this.batchHdrID, this.batchid, nwDiag.attchmntNmTextBox.Text, "");
          long attchID = Global.getAttchmntID(nwDiag.attchmntNmTextBox.Text, this.batchid, this.batchHdrID);
          string extnsn = Global.mnFrm.cmCde.myComputer.FileSystem.GetFileInfo(nwDiag.fileNmTextBox.Text).Extension;
          if (Global.mnFrm.cmCde.copyAFile(attchID, Global.mnFrm.cmCde.getAttnDocsImgsDrctry(), nwDiag.fileNmTextBox.Text) == true)
          {
            //Global.mnFrm.cmCde.upldImgsFTP(4, Global.mnFrm.cmCde.getAttnDocsImgsDrctry(), attchID.ToString() + extnsn); 
            Global.updateAttachment(attchID, this.batchid, this.batchHdrID, nwDiag.attchmntNmTextBox.Text, attchID.ToString() + extnsn);
          }
        }
      }
      this.gotoButton_Click(this.gotoButton, e);
    }

    private void editButton_Click(object sender, EventArgs e)
    {
      if (this.attchmntsListView.SelectedItems.Count <= 0)
      {
        Global.mnFrm.cmCde.showMsg("Please select an Item First!", 0);
        return;
      }
      string oldFile = Global.mnFrm.cmCde.getAttnDocsImgsDrctry() + @"\" +
        this.attchmntsListView.SelectedItems[0].SubItems[2].Text;
      string oldExtn = this.attchmntsListView.SelectedItems[0].SubItems[2].Text;
      addAttchmntDiag nwDiag = new addAttchmntDiag();
      nwDiag.attchmntIDTextBox.Text = this.attchmntsListView.SelectedItems[0].SubItems[3].Text;
      nwDiag.batchID = this.batchid;
      nwDiag.attchmntNmTextBox.Text = this.attchmntsListView.SelectedItems[0].SubItems[1].Text;
      nwDiag.fileNmTextBox.Text = oldFile;
      DialogResult dgrs = nwDiag.ShowDialog();
      if (dgrs == DialogResult.OK)
      {
        if (Global.mnFrm.cmCde.myComputer.FileSystem.FileExists(
          nwDiag.fileNmTextBox.Text) == true)
        {
          //Global.createAttachment(this.batchid, nwDiag.attchmntNmTextBox.Text, "");
          long attchID = long.Parse(nwDiag.attchmntIDTextBox.Text);
          if (nwDiag.fileNmTextBox.Text != oldFile)
          {
            string extnsn = Global.mnFrm.cmCde.myComputer.FileSystem.GetFileInfo(nwDiag.fileNmTextBox.Text).Extension;
            if (Global.mnFrm.cmCde.copyAFile(attchID, Global.mnFrm.cmCde.getAttnDocsImgsDrctry(), nwDiag.fileNmTextBox.Text) == true)
            {
              //Global.mnFrm.cmCde.upldImgsFTP(4, Global.mnFrm.cmCde.getAttnDocsImgsDrctry(), attchID.ToString() + extnsn); 
              Global.updateAttachment(attchID, this.batchid, this.batchHdrID, nwDiag.attchmntNmTextBox.Text, attchID.ToString() + extnsn);
            }
          }
          else
          {
            Global.mnFrm.cmCde.upldImgsFTP(10, Global.mnFrm.cmCde.getAttnDocsImgsDrctry(), oldExtn);
            Global.updateAttachment(attchID, this.batchid, this.batchHdrID, nwDiag.attchmntNmTextBox.Text, oldExtn);
          }
        }
      }
      this.gotoButton_Click(this.gotoButton, e);
    }

    private void delButton_Click(object sender, EventArgs e)
    {
      if (this.attchmntsListView.SelectedItems.Count <= 0)
      {
        Global.mnFrm.cmCde.showMsg("Please select an Item First!", 0);
        return;
      }
      if (Global.mnFrm.cmCde.showMsg("NB: This action cannot be undone!\r\n" +
 "Are you sure you want to delete the selected Attachment?", 1) == DialogResult.No)
      {
        Global.mnFrm.cmCde.showMsg("Operation Cancelled!", 4);
        return;
      }
      string oldFile = Global.mnFrm.cmCde.getAttnDocsImgsDrctry() + @"\" +
        this.attchmntsListView.SelectedItems[0].SubItems[2].Text;
      if (Global.mnFrm.cmCde.deleteAFile(oldFile) == true)
      {
        Global.deleteAttchmnt(long.Parse(this.attchmntsListView.SelectedItems[0].SubItems[3].Text),
          this.attchmntsListView.SelectedItems[0].SubItems[1].Text);
      }
      this.gotoButton_Click(this.gotoButton, e);
    }

    private void openFileButton_Click(object sender, EventArgs e)
    {
      if (this.attchmntsListView.SelectedItems.Count > 0)
      {
        //      Global.mnFrm.cmCde.showMsg(Global.mnFrm.cmCde.getAttnDocsImgsDrctry() +
        //@"\" + this.attchmntsListView.SelectedItems[0].SubItems[2].Text, 0);
        Global.mnFrm.cmCde.dwnldImgsFTP(10, Global.mnFrm.cmCde.getAttnDocsImgsDrctry(),
          this.attchmntsListView.SelectedItems[0].SubItems[2].Text);
        System.Diagnostics.Process.Start(Global.mnFrm.cmCde.getAttnDocsImgsDrctry() +
  @"\" + this.attchmntsListView.SelectedItems[0].SubItems[2].Text);
      }
    }

    private void attchmntsListView_DoubleClick(object sender, EventArgs e)
    {
      if (this.attchmntsListView.SelectedItems.Count > 0)
      {
        this.openFileButton.PerformClick();
      }
    }
  }
}