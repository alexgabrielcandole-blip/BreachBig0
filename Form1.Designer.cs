#nullable disable
using System.Drawing;
using System.Windows.Forms;

namespace BreachBigO
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlTerminal = new Panel();
            rtbTerminal = new RichTextBox();
            pnlHeader = new Panel();
            lblBigO = new Label();
            lblNode = new Label();
            pnlBottom = new Panel();
            txtInput = new TextBox();
            lblPrompt = new Label();
            lblNext = new Label();
            pnlSide = new Panel();
            btnBrute = new Button();
            btnHint = new Button();
            lblCompare = new Label();
            lblCompareTitle = new Label();
            lblTracePct = new Label();
            prgTrace = new ProgressBar();
            lblTraceTitle = new Label();
            lblSteps = new Label();
            lblStepsTitle = new Label();
            pnlTerminal.SuspendLayout();
            pnlHeader.SuspendLayout();
            pnlBottom.SuspendLayout();
            pnlSide.SuspendLayout();
            SuspendLayout();
            // pnlTerminal (center area, gives the terminal some padding)
            pnlTerminal.BackColor = Color.FromArgb(8, 10, 8);
            pnlTerminal.Controls.Add(rtbTerminal);
            pnlTerminal.Dock = DockStyle.Fill;
            pnlTerminal.Location = new Point(0, 52);
            pnlTerminal.Name = "pnlTerminal";
            pnlTerminal.Padding = new Padding(14, 10, 14, 10);
            pnlTerminal.Size = new Size(730, 472);
            pnlTerminal.TabIndex = 3;
            // rtbTerminal (read-only output, green on black)
            rtbTerminal.BackColor = Color.FromArgb(8, 10, 8);
            rtbTerminal.BorderStyle = BorderStyle.None;
            rtbTerminal.Dock = DockStyle.Fill;
            rtbTerminal.Font = new Font("Consolas", 11F);
            rtbTerminal.ForeColor = Color.FromArgb(0, 255, 120);
            rtbTerminal.HideSelection = false;
            rtbTerminal.Location = new Point(14, 10);
            rtbTerminal.Name = "rtbTerminal";
            rtbTerminal.ReadOnly = true;
            rtbTerminal.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbTerminal.Size = new Size(702, 452);
            rtbTerminal.TabIndex = 0;
            rtbTerminal.TabStop = false;
            rtbTerminal.Text = "";
            // pnlHeader (node title + Big-O tag)
            pnlHeader.BackColor = Color.FromArgb(14, 20, 16);
            pnlHeader.Controls.Add(lblBigO);
            pnlHeader.Controls.Add(lblNode);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(730, 52);
            pnlHeader.TabIndex = 1;
            // lblBigO
            lblBigO.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBigO.AutoSize = true;
            lblBigO.Font = new Font("Consolas", 14F, FontStyle.Bold);
            lblBigO.ForeColor = Color.FromArgb(255, 193, 7);
            lblBigO.Location = new Point(620, 14);
            lblBigO.Name = "lblBigO";
            lblBigO.Size = new Size(98, 22);
            lblBigO.TabIndex = 1;
            lblBigO.Text = "[ O(1) ]";
            // lblNode
            lblNode.AutoSize = true;
            lblNode.Font = new Font("Consolas", 14F, FontStyle.Bold);
            lblNode.ForeColor = Color.FromArgb(0, 255, 120);
            lblNode.Location = new Point(16, 14);
            lblNode.Name = "lblNode";
            lblNode.Size = new Size(329, 22);
            lblNode.TabIndex = 0;
            lblNode.Text = "NODE 1/5 — DIRECT ACCESS";
            // pnlBottom (NEXT bar + prompt + input)
            pnlBottom.BackColor = Color.FromArgb(14, 20, 16);
            pnlBottom.Controls.Add(txtInput);
            pnlBottom.Controls.Add(lblPrompt);
            pnlBottom.Controls.Add(lblNext);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 524);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(730, 96);
            pnlBottom.TabIndex = 2;
            // txtInput (the player types here)
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.BackColor = Color.FromArgb(4, 6, 4);
            txtInput.BorderStyle = BorderStyle.FixedSingle;
            txtInput.Font = new Font("Consolas", 12F);
            txtInput.ForeColor = Color.FromArgb(0, 255, 120);
            txtInput.Location = new Point(176, 48);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(538, 27);
            txtInput.TabIndex = 0;
            txtInput.KeyDown += txtInput_KeyDown;
            // lblPrompt
            lblPrompt.AutoSize = true;
            lblPrompt.Font = new Font("Consolas", 12F, FontStyle.Bold);
            lblPrompt.ForeColor = Color.FromArgb(0, 255, 120);
            lblPrompt.Location = new Point(16, 51);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(154, 19);
            lblPrompt.TabIndex = 1;
            lblPrompt.Text = "root@node1>";
            // lblNext (always tells the player what to do now)
            lblNext.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblNext.Font = new Font("Consolas", 10.5F, FontStyle.Bold);
            lblNext.ForeColor = Color.FromArgb(255, 193, 7);
            lblNext.Location = new Point(16, 10);
            lblNext.Name = "lblNext";
            lblNext.Size = new Size(698, 28);
            lblNext.TabIndex = 2;
            lblNext.Text = "NEXT: type 'help' to begin.";
            lblNext.TextAlign = ContentAlignment.MiddleLeft;
            // pnlSide (step counter, trace meter, hint, brute force)
            pnlSide.BackColor = Color.FromArgb(14, 20, 16);
            pnlSide.Controls.Add(btnBrute);
            pnlSide.Controls.Add(btnHint);
            pnlSide.Controls.Add(lblCompare);
            pnlSide.Controls.Add(lblCompareTitle);
            pnlSide.Controls.Add(lblTracePct);
            pnlSide.Controls.Add(prgTrace);
            pnlSide.Controls.Add(lblTraceTitle);
            pnlSide.Controls.Add(lblSteps);
            pnlSide.Controls.Add(lblStepsTitle);
            pnlSide.Dock = DockStyle.Right;
            pnlSide.Location = new Point(730, 0);
            pnlSide.Name = "pnlSide";
            pnlSide.Size = new Size(250, 620);
            pnlSide.TabIndex = 0;
            // btnBrute (reveals the answer for +25% trace, asks first)
            btnBrute.BackColor = Color.FromArgb(40, 24, 24);
            btnBrute.Cursor = Cursors.Hand;
            btnBrute.FlatAppearance.BorderColor = Color.FromArgb(255, 80, 80);
            btnBrute.FlatStyle = FlatStyle.Flat;
            btnBrute.Font = new Font("Consolas", 11F, FontStyle.Bold);
            btnBrute.ForeColor = Color.FromArgb(255, 80, 80);
            btnBrute.Location = new Point(16, 346);
            btnBrute.Name = "btnBrute";
            btnBrute.Size = new Size(218, 44);
            btnBrute.TabIndex = 2;
            btnBrute.TabStop = false;
            btnBrute.Text = "BRUTE FORCE (+25 trace)";
            btnBrute.UseVisualStyleBackColor = false;
            btnBrute.Click += btnBrute_Click;
            // btnHint (costs trace)
            btnHint.BackColor = Color.FromArgb(30, 40, 34);
            btnHint.Cursor = Cursors.Hand;
            btnHint.FlatAppearance.BorderColor = Color.FromArgb(255, 193, 7);
            btnHint.FlatStyle = FlatStyle.Flat;
            btnHint.Font = new Font("Consolas", 11F, FontStyle.Bold);
            btnHint.ForeColor = Color.FromArgb(255, 193, 7);
            btnHint.Location = new Point(16, 290);
            btnHint.Name = "btnHint";
            btnHint.Size = new Size(218, 44);
            btnHint.TabIndex = 1;
            btnHint.TabStop = false;
            btnHint.Text = "HINT  (+5 trace)";
            btnHint.UseVisualStyleBackColor = false;
            btnHint.Click += btnHint_Click;
            // lblCompare
            lblCompare.AutoSize = true;
            lblCompare.Font = new Font("Consolas", 16F, FontStyle.Bold);
            lblCompare.ForeColor = Color.FromArgb(0, 255, 120);
            lblCompare.Location = new Point(16, 218);
            lblCompare.Name = "lblCompare";
            lblCompare.Size = new Size(87, 25);
            lblCompare.TabIndex = 6;
            lblCompare.Text = "0 / --";
            // lblCompareTitle
            lblCompareTitle.AutoSize = true;
            lblCompareTitle.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblCompareTitle.ForeColor = Color.FromArgb(90, 160, 120);
            lblCompareTitle.Location = new Point(16, 196);
            lblCompareTitle.Name = "lblCompareTitle";
            lblCompareTitle.Size = new Size(168, 16);
            lblCompareTitle.TabIndex = 5;
            lblCompareTitle.Text = "YOUR STEPS / BEST";
            // lblTracePct
            lblTracePct.AutoSize = true;
            lblTracePct.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblTracePct.ForeColor = Color.FromArgb(0, 255, 120);
            lblTracePct.Location = new Point(16, 150);
            lblTracePct.Name = "lblTracePct";
            lblTracePct.Size = new Size(35, 18);
            lblTracePct.TabIndex = 4;
            lblTracePct.Text = "0%";
            // prgTrace
            prgTrace.Location = new Point(16, 124);
            prgTrace.Name = "prgTrace";
            prgTrace.Size = new Size(218, 22);
            prgTrace.Style = ProgressBarStyle.Continuous;
            prgTrace.TabIndex = 3;
            // lblTraceTitle
            lblTraceTitle.AutoSize = true;
            lblTraceTitle.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblTraceTitle.ForeColor = Color.FromArgb(90, 160, 120);
            lblTraceTitle.Location = new Point(16, 100);
            lblTraceTitle.Name = "lblTraceTitle";
            lblTraceTitle.Size = new Size(112, 16);
            lblTraceTitle.TabIndex = 2;
            lblTraceTitle.Text = "TRACE LEVEL";
            // lblSteps
            lblSteps.AutoSize = true;
            lblSteps.Font = new Font("Consolas", 28F, FontStyle.Bold);
            lblSteps.ForeColor = Color.FromArgb(0, 255, 120);
            lblSteps.Location = new Point(12, 36);
            lblSteps.Name = "lblSteps";
            lblSteps.Size = new Size(42, 44);
            lblSteps.TabIndex = 1;
            lblSteps.Text = "0";
            // lblStepsTitle
            lblStepsTitle.AutoSize = true;
            lblStepsTitle.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblStepsTitle.ForeColor = Color.FromArgb(90, 160, 120);
            lblStepsTitle.Location = new Point(16, 16);
            lblStepsTitle.Name = "lblStepsTitle";
            lblStepsTitle.Size = new Size(112, 16);
            lblStepsTitle.TabIndex = 0;
            lblStepsTitle.Text = "STEP COUNTER";
            // Form1
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(8, 10, 8);
            ClientSize = new Size(980, 620);
            Controls.Add(pnlTerminal);
            Controls.Add(pnlHeader);
            Controls.Add(pnlBottom);
            Controls.Add(pnlSide);
            MinimumSize = new Size(900, 560);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "BREACH — Big-O Terminal";
            Load += Form1_Load;
            pnlTerminal.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            pnlSide.ResumeLayout(false);
            pnlSide.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTerminal;
        private RichTextBox rtbTerminal;
        private Panel pnlHeader;
        private Label lblBigO;
        private Label lblNode;
        private Panel pnlBottom;
        private TextBox txtInput;
        private Label lblPrompt;
        private Label lblNext;
        private Panel pnlSide;
        private Button btnBrute;
        private Button btnHint;
        private Label lblCompare;
        private Label lblCompareTitle;
        private Label lblTracePct;
        private ProgressBar prgTrace;
        private Label lblTraceTitle;
        private Label lblSteps;
        private Label lblStepsTitle;
    }
}