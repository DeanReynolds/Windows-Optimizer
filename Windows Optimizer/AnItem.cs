﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Optimizer {
    public partial class AnItem : UserControl {
        public static Image Check, Warn;

        Func<AnItem, bool> _check;
        Action<AnItem> _ifIncomplete, _ifComplete, _apply;

        public AnItem(Func<AnItem, bool> check, Action<AnItem> ifIncomplete, Action<AnItem> ifComplete, Action<AnItem> apply) {
            InitializeComponent();
            _check = check;
            _ifIncomplete = ifIncomplete;
            _ifComplete = ifComplete;
            _apply = apply;
        }

        public void SetText(string text, Color color, FontStyle style) {
            label1.Text = text;
            label1.ForeColor = color;
            label1.Font = new Font(label1.Font, style);
        }

        public void GoCheck() {
            pictureBox1.Image = Check;
        }
        public void GoWarn() {
            pictureBox1.Image = Warn;
        }

        public bool RunCheck() {
            if (_check(this)) {
                _ifComplete(this);
                return true;
            }
            _ifIncomplete(this);
            return false;
        }

        public void Apply() {
            _apply(this);
        }
    }
}
