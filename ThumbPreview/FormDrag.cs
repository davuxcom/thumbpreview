using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThumbPreview
{
    class FormDrag
    { 
        Form _form;
        bool isDragging = false;
        int oldX, oldY = 0;

        public FormDrag(Form form) {
            _form = form;

            if (_form != null)
            {
                _form.MouseDown += new MouseEventHandler(_form_MouseDown);
                _form.MouseUp += new MouseEventHandler(_form_MouseUp);
                _form.MouseMove += new MouseEventHandler(_form_MouseMove);
            }
        }

        void _form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                _form.Top = _form.Top + (e.Y - oldY);

                _form.Left = _form.Left + (e.X - oldX);
            }
            
        }

        void _form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        void _form_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            oldX = e.X;
            oldY = e.Y;
        }
    }
}
