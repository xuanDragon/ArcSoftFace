using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcSoftFace.Utils;

namespace ArcSoftFace
{
    public partial class FormStudent : Form
    {
        string PhotoPath = null;
        private long maxSize = 1024 * 1024 * 2;
        public FormStudent()
        {
            InitializeComponent();
            if (AuthenticationForm.getClass != " ALL STUDENTS")
            {
                CnotextBox.ReadOnly = true;
                CnametextBox.ReadOnly = true;
                string str1 = AuthenticationForm.getClass;
                string[] strArray1;
                strArray1 = str1.Split(' ');
                CnotextBox.Text = strArray1[0];
                CnametextBox.Text = strArray1[1];
            }
            string str2 = AuthenticationForm.getStudent;
            string[] strArray2;
            strArray2 = str2.Split(' ');
            textBoxOldSno.Text = strArray2[0];
            textBoxOldSname.Text = strArray2[1];
            string gender = "";
            using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
            {
                string strLine = sr.ReadLine();
                string[] strStudent;
                while ((strLine = sr.ReadLine()) != null)
                {
                    strStudent = strLine.Split(',');
                    if (strStudent[0] == strArray2[0])
                    {
                        gender = strStudent[2];
                        break;
                    }

                }
            }
            textBoxOldGender.Text = gender;
        }

        private bool CheckImage(string imagePath)
        {
            if (imagePath == null)
            {
                MessageBox.Show("图片不存在，请确认后再导入\r\n");
                return false;
            }
            try
            {
                //判断图片是否正常，如将其他文件把后缀改为.jpg，这样就会报错
                Image image = ImageUtil.readFromFile(imagePath);
                if (image == null)
                {
                    throw new Exception();
                }
                else
                {
                    image.Dispose();
                }
            }
            catch
            {
                MessageBox.Show(string.Format("{0} 图片格式有问题，请确认后再导入\r\n", imagePath));
                return false;
            }
            FileInfo fileCheck = new FileInfo(imagePath);
            if (fileCheck.Exists == false)
            {
                MessageBox.Show(string.Format("{0} 不存在\r\n", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length > maxSize)
            {
                MessageBox.Show(string.Format("{0} 图片大小超过2M，请压缩后再导入\r\n", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length < 2)
            {
                MessageBox.Show(string.Format("{0} 图像质量太小，请重新选择\r\n", fileCheck.Name));
                return false;
            }
            return true;
        }

        private void PhotoChoose_Click(object sender, EventArgs e)
        {
            OpenFileDialog photo = new OpenFileDialog();
            photo.Title = "选择图片";
            photo.Filter = "图片文件(*.jpg)|*.jpg";
            if (photo.ShowDialog() == System.Windows.Forms.DialogResult.OK && CheckImage(photo.FileName))
            {
                FilePhoto.Image = Image.FromFile(photo.FileName);
                FilePhoto.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            PhotoPath = photo.FileName;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string cno = CnotextBox.Text;
            string cname = CnametextBox.Text;
            string sno = SnotextBox.Text;
            string sname = SnametextBox.Text;
            string gender = GendertextBox.Text;
            int record = 0; //计算课程的签到记录
            int lines = 0;  //统计Student的个数
            using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\Class.csv"))
            {
                string strLine = sr.ReadLine();
                string[] strArray;
                while ((strLine = sr.ReadLine()) != null)
                {
                    strArray = strLine.Split(',');
                    if (strArray[0] == cno)
                        record = Convert.ToInt32(strArray[2]);
                }
            }
            using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
            {
                string strline = sr.ReadLine();
                while ((strline = sr.ReadLine()) != null)
                {
                    lines++;
                }
            }
            if (PhotoPath != null && sname != null && cno != null && sno != null && gender != null)
            {
                string[] strSno = new string[lines];
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        int i = 0;
                        strArray = strLine.Split(',');
                        strSno[i] = strArray[0];
                        i++;
                    }
                }
                FileStream fs1 = new FileStream("..\\..\\..\\Database\\Student\\Student.csv", FileMode.Append);
                FileStream fs2 = new FileStream("..\\..\\..\\Database\\Class\\List\\" + cno + ".csv", FileMode.Append);
                using (StreamWriter brout1 = new StreamWriter(fs1))
                {
                    bool flag = true;
                    foreach (string s in strSno)
                    {
                        if (s == sno)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        brout1.WriteLine(sno + "," + sname + "," + gender);
                    }

                }
                using (StreamWriter brout2 = new StreamWriter(fs2))
                {
                    string str = sno;
                    for (int i = 0; i < record; i++)
                    {
                        str += ",0";
                    }
                    brout2.WriteLine(str);
                }
                File.Copy(PhotoPath, "..\\..\\..\\Database\\Student\\Image\\" + sno + ".jpg", true);
                MessageBox.Show("添加学生成功");
            }
            else
            {
                MessageBox.Show("请输入完整信息");
            }
            PhotoPath = null;
        }

        private void Modify_Click(object sender, EventArgs e)
        {
            string New_Sno = textBoxSno.Text;
            string New_Sname = textBoxSname.Text;
            string New_Gender = textBoxGender.Text;
            string str = AuthenticationForm.getStudent;
            string[] StrArray;
            StrArray = str.Split(' ');
            string Old_Sno = StrArray[0], Old_Sname = StrArray[1];
            string txt = "Sno,Sname,Gender" + "\r\n";
            //将学生表中对应信息进行修改
            using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
            {
                string strLine = sr.ReadLine();
                string[] strStudent;
                while ((strLine = sr.ReadLine()) != null)
                {
                    strStudent = strLine.Split(',');
                    if (strStudent[0] == Old_Sno)
                    {
                        txt += New_Sno + "," + New_Sname + "," + New_Gender;
                        txt += "\r\n";
                    }
                    else
                    {
                        txt += strLine + "\r\n";
                    }
                }
            }
            //将txt重新写入
            using (StreamWriter writer = new StreamWriter("..\\..\\..\\Database\\Student\\Student.csv"))
            {
                writer.Write(txt);
            }
            //如果学号进行了更改则需要对该学生所在课程的课程表进行修改，以及照片名进行修改
            if (Old_Sno != New_Sno)
            {
                int lines = 0; //用来统计课程数，方便循环查找
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\Class.csv"))
                {
                    string strline = sr.ReadLine();
                    while ((strline = sr.ReadLine()) != null)
                    {
                        lines++;
                    }
                }
                string[] Course = new string[lines];
                //将所有课程课程号读入数组
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\Class.csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray;
                    int i = 0;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strArray = strLine.Split(',');
                        Course[i] = strArray[0];
                        i++;
                    }
                }
                //在所有课程中寻找该学生信息，若有则修改
                foreach (string c in Course)
                {
                    int record = 0;
                    string text = "Sno";
                    //统计该课程的签到次数
                    using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\Class.csv"))
                    {
                        string strLine = sr.ReadLine();
                        string[] strArray;
                        while ((strLine = sr.ReadLine()) != null)
                        {
                            strArray = strLine.Split(',');
                            if (strArray[0] == c)
                                record = Convert.ToInt32(strArray[2]);
                        }
                    }
                    for (int i = 0; i < record; i++)
                    {
                        string str1 = (",Record" + (i + 1).ToString());
                        text += str1;
                    }
                    text += "\r\n";
                    //将课程数据全部读出读入text
                    using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\List\\" + c + ".csv"))
                    {
                        string strLine = sr.ReadLine();
                        string[] strClass;
                        while ((strLine = sr.ReadLine()) != null)
                        {
                            strClass = strLine.Split(',');
                            if (strClass[0] == Old_Sno)
                            {
                                text += New_Sno;
                                for (int i = 1; i < record + 1; i++)
                                {
                                    text += "," + strClass[i].ToString();
                                }
                                text += "\r\n";
                            }
                            else
                            {
                                text += strLine + "\r\n";
                            }
                        }
                    }
                    //将text重新写入
                    using (StreamWriter writer = new StreamWriter("..\\..\\..\\Database\\Class\\List\\" + c + ".csv"))
                    {
                        writer.Write(text);
                    }
                }
                if (PhotoPath != null)
                {
                    File.Delete("..\\..\\..\\Database\\Student\\Image\\" + Old_Sno + ".jpg");
                    File.Copy(PhotoPath, "..\\..\\..\\Database\\Student\\Image\\" + New_Sno + ".jpg", true);
                }
                else
                {
                    File.Copy("..\\..\\..\\Database\\Student\\Image\\" + Old_Sno + ".jpg",
                        "..\\..\\..\\Database\\Student\\Image\\" + New_Sno + ".jpg", true);
                    File.Delete("..\\..\\..\\Database\\Student\\Image\\" + Old_Sno + ".jpg");
                }
            }
            //如果学号未改之更改照片，则直接将原照片改成新照片
            if (PhotoPath != null)
            {
                File.Copy(PhotoPath, "..\\..\\..\\Database\\Student\\Image\\" + New_Sno + ".jpg", true);
            }
            MessageBox.Show("修改成功");
        }
    }
    
}
