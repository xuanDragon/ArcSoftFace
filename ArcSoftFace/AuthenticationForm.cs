using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ArcSoftFace.SDKModels;
using ArcSoftFace.SDKUtil;
using ArcSoftFace.Utils;
using ArcSoftFace.Entity;
using AForge.Video.DirectShow;

namespace ArcSoftFace
{
    public partial class AuthenticationForm : Form
    {
        #region Data
        class Class
        {
            public string Cno;      //课程号
            public string Cname;    //课程名
            public int recordNum;   //考勤记录数
            public Class(string Cno, string Cname, int recordNum)
            {
                this.Cno = Cno;
                this.Cname = Cname;
                this.recordNum = recordNum;
            }
        }
        class Student
        {
            public string Sno;      //学号
            public string Sname;    //姓名
            public string Gender;   //性别
            public int Attendance;  //在当前课堂下的到课次数
            public Student(string Sno, string Sname, string Gender, int Attendance)
            {
                this.Sno = Sno;
                this.Sname = Sname;
                this.Gender = Gender;
                this.Attendance = Attendance;
            }
        }
        class Record
        {
            public string Rname;            //记录名
            public int Attendance;          //出席人数
            public List<bool> list;         //出席学生列表（列表长应等于学生数）
            public Record(string Rname, int Attendance)
            {
                this.Rname = Rname;
                this.Attendance = Attendance;
                list = new List<bool>();
            }
        }
        private List<Class> classes = new List<Class>();
        private List<Student> students = new List<Student>();
        private List<Record> records = new List<Record>();
        #endregion

        #region Init
        public AuthenticationForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            //初始化引擎
            InitEngines();
            //隐藏摄像头图像窗口
            rgbVideoSource.Hide();
            irVideoSource.Hide();


            importClass();
        }
        #endregion

        #region Import
        private void importClass()
        {
            using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\Class.csv"))
            {
                string strLine = sr.ReadLine();
                string[] strArray;
                classes.Clear();
                classes.Add(new Class("", "ALL STUDENTS", 0));
                while ((strLine = sr.ReadLine()) != null)
                {
                    strArray = strLine.Split(',');
                    classes.Add(new Class(strArray[0], strArray[1], Convert.ToInt32(strArray[2])));
                }
            }
            comboBoxClass.Items.Clear();
            foreach (Class c in classes)
            {
                comboBoxClass.Items.Add(c.Cno + " " + c.Cname);
            }
            comboBoxClass.SelectedIndex = 0;
        }

        private void importStudent(int index)
        {
            if (index == 0)
            {
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray;
                    students.Clear();
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strArray = strLine.Split(',');
                        students.Add(new Student(strArray[0], strArray[1], strArray[2], 0));
                    }
                }
                comboBoxStudent.Items.Clear();
                foreach (Student s in students)
                {
                    comboBoxStudent.Items.Add(s.Sno + " " + s.Sname);
                }
                if (students.Count > 0) comboBoxStudent.SelectedIndex = 0;
                labelClassStuNum.Text = "学生数：" + students.Count.ToString();
            }
            else
            {
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\List\\" + classes[index].Cno + ".csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray;
                    Student s;
                    students.Clear();
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strArray = strLine.Split(',');
                        s = new Student(strArray[0], "", "", 0);
                        for (int i = 0; i < classes[index].recordNum; i++)
                        {
                            if (strArray[i + 1] == "1")
                            {
                                s.Attendance++;
                            }
                        }
                        students.Add(s);
                    }
                }
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strArray = strLine.Split(',');
                        students.ForEach(s =>
                        {
                            if (s.Sno == strArray[0])
                            {
                                s.Sname = strArray[1];
                                s.Gender = strArray[2];
                            }
                        });
                    }
                }
                comboBoxStudent.Items.Clear();
                foreach (Student s in students)
                {
                    comboBoxStudent.Items.Add(s.Sno + " " + s.Sname);
                }
                if (students.Count > 0) comboBoxStudent.SelectedIndex = 0;
                labelClassStuNum.Text = "学生数：" + students.Count.ToString();
            }
        }

        private void importRecord(int index)
        {
            if (index == 0)
            {
                records.Clear();
                buttonStart.Enabled = false;
                comboBoxRecord.Items.Clear();
                comboBoxRecord.Text = "";
                labelRecordAttendance.Text = "已到课人数：无考勤记录！";
                labelRecordSkip.Text = "未到课人数：";
            }
            else
            {
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\List\\" + classes[index].Cno + ".csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray = strLine.Split(',');
                    records.Clear();
                    for (int i = 0; i < classes[index].recordNum; i++)
                    {
                        records.Add(new Record(strArray[i + 1], 0));
                    }
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strArray = strLine.Split(',');
                        for (int i = 0; i < classes[index].recordNum; i++)
                        {
                            if (strArray[i + 1] == "1")
                            {
                                records[i].Attendance++;
                                records[i].list.Add(true);
                            }
                            else
                            {
                                records[i].list.Add(false);
                            }
                        }
                    }
                }
                comboBoxRecord.Items.Clear();
                foreach (Record r in records)
                {
                    comboBoxRecord.Items.Add(r.Rname);
                }
                if (records != null && records.Count > 0)
                {
                    comboBoxRecord.SelectedIndex = 0;
                }
                else
                {
                    buttonStart.Enabled = false;
                    comboBoxRecord.Text = "";
                    labelRecordAttendance.Text = "已到课人数：无考勤记录！";
                    labelRecordSkip.Text = "未到课人数：";
                }
            }
        }

        private void importImage(string Sno)
        {
            string imagePath = "..\\..\\..\\Database\\Student\\Image\\" + Sno + ".jpg";
            Image image = ImageUtil.readFromFile(imagePath);
            if (image == null)
            {
                pictureBoxImage.Image = null;
                pictureBoxFace.Image = null;
                return;
            }
            if (image.Width > 1536 || image.Height > 1536)
            {
                image = ImageUtil.ScaleImage(image, 1536, 1536);
            }
            if (image == null) return;
            //调整图像宽度，需要宽度为4的倍数
            if (image.Width % 4 != 0)
            {
                image = ImageUtil.ScaleImage(image, image.Width - (image.Width % 4), image.Height);
            }
            //调整图片数据，非常重要
            ImageInfo imageInfo = ImageUtil.ReadBMP(image);
            if (imageInfo == null)
            {
                MessageBox.Show("图像数据获取失败，请稍后重试!");
                AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                AppendText("\n");
                return;
            }
            //人脸检测
            ASF_MultiFaceInfo multiFaceInfo = FaceUtil.DetectFace(pImageEngine, imageInfo);
            //年龄检测
            int retCode_Age = -1;
            ASF_AgeInfo ageInfo = FaceUtil.AgeEstimation(pImageEngine, imageInfo, multiFaceInfo, out retCode_Age);
            //性别检测
            int retCode_Gender = -1;
            ASF_GenderInfo genderInfo = FaceUtil.GenderEstimation(pImageEngine, imageInfo, multiFaceInfo, out retCode_Gender);

            //3DAngle检测
            int retCode_3DAngle = -1;
            ASF_Face3DAngle face3DAngleInfo = FaceUtil.Face3DAngleDetection(pImageEngine, imageInfo, multiFaceInfo, out retCode_3DAngle);

            MemoryUtil.Free(imageInfo.imgData);

            //判断检测结果

            if (multiFaceInfo.faceNum < 1)
            {
                image = ImageUtil.ScaleImage(image, pictureBoxImage.Width, pictureBoxImage.Height);
                image1Feature = IntPtr.Zero;
                pictureBoxImage.Image = image;
                AppendText(string.Format("未检测出人脸!\r\n"));
                AppendText(string.Format("----------------检测结束----------------\r\n"));
                return;
            }
            MRECT temp = new MRECT();
            int ageTemp = 0;
            int genderTemp = 0;
            int rectTemp = 0;

            //标记出检测到的人脸
            for (int i = 0; i < multiFaceInfo.faceNum; i++)
            {
                MRECT rect = MemoryUtil.PtrToStructure<MRECT>(multiFaceInfo.faceRects + MemoryUtil.SizeOf<MRECT>() * i);
                int orient = MemoryUtil.PtrToStructure<int>(multiFaceInfo.faceOrients + MemoryUtil.SizeOf<int>() * i);
                int age = 0;

                if (retCode_Age != 0)
                {
                    AppendText(string.Format("年龄检测失败，返回{0}!\n\n", retCode_Age));
                }
                else
                {
                    age = MemoryUtil.PtrToStructure<int>(ageInfo.ageArray + MemoryUtil.SizeOf<int>() * i);
                }

                int gender = -1;
                if (retCode_Gender != 0)
                {
                    AppendText(string.Format("性别检测失败，返回{0}!\n\n", retCode_Gender));
                }
                else
                {
                    gender = MemoryUtil.PtrToStructure<int>(genderInfo.genderArray + MemoryUtil.SizeOf<int>() * i);
                }

                int face3DStatus = -1;
                float roll = 0f;
                float pitch = 0f;
                float yaw = 0f;
                if (retCode_3DAngle != 0)
                {
                    AppendText(string.Format("3DAngle检测失败，返回{0}!\n\n", retCode_3DAngle));
                }
                else
                {
                    //角度状态 非0表示人脸不可信
                    face3DStatus = MemoryUtil.PtrToStructure<int>(face3DAngleInfo.status + MemoryUtil.SizeOf<int>() * i);
                    //roll为侧倾角，pitch为俯仰角，yaw为偏航角
                    roll = MemoryUtil.PtrToStructure<float>(face3DAngleInfo.roll + MemoryUtil.SizeOf<float>() * i);
                    pitch = MemoryUtil.PtrToStructure<float>(face3DAngleInfo.pitch + MemoryUtil.SizeOf<float>() * i);
                    yaw = MemoryUtil.PtrToStructure<float>(face3DAngleInfo.yaw + MemoryUtil.SizeOf<float>() * i);
                }

                int rectWidth = rect.right - rect.left;
                int rectHeight = rect.bottom - rect.top;

                //查找最大人脸
                if (rectWidth * rectHeight > rectTemp)
                {
                    rectTemp = rectWidth * rectHeight;
                    temp = rect;
                    ageTemp = age;
                    genderTemp = gender;
                }
                AppendText(string.Format("人脸{0}特征:Age:{1} Gender:{2}\r\n", i + 1, age, (gender >= 0 ? gender.ToString() : "")));
            }

            AppendText(string.Format("人脸数量:{0}\r\n", multiFaceInfo.faceNum));

            DateTime detectEndTime = DateTime.Now;
            AppendText(string.Format("----------------检测结束----------------\r\n"));
            //ASF_SingleFaceInfo singleFaceInfo = new ASF_SingleFaceInfo();
            Image image_f = image;
            //获取缩放比例
            float scaleRate = ImageUtil.getWidthAndHeight(image.Width, image.Height, pictureBoxImage.Width, pictureBoxImage.Height);
            //缩放图片
            image = ImageUtil.ScaleImage(image, pictureBoxImage.Width, pictureBoxImage.Height);
            //添加标记
            image = ImageUtil.MarkRectAndString(image, (int)(temp.left * scaleRate), (int)(temp.top * scaleRate), (int)(temp.right * scaleRate) - (int)(temp.left * scaleRate), (int)(temp.bottom * scaleRate) - (int)(temp.top * scaleRate), ageTemp, genderTemp, pictureBoxImage.Width);

            //显示标记后的图像
            pictureBoxImage.Image = image;

            MRECT rect_f = MemoryUtil.PtrToStructure<MRECT>(multiFaceInfo.faceRects);
            image_f = ImageUtil.CutImage(image_f, rect_f.left, rect_f.top, rect_f.right, rect_f.bottom);
            image_f = ImageUtil.ScaleImage(image_f, pictureBoxFace.Width, pictureBoxFace.Height);
            pictureBoxFace.Image = image_f;
            //显示人脸
            /*
            if (image == null)
            {
                image = ImageUtil.readFromFile(imagePath);
                if (image.Width > 1536 || image.Height > 1536)
                {
                    image = ImageUtil.ScaleImage(image, 1536, 1536);
                }
            }
            */
        }

        private void importFeature()
        {
            imagesFeatureList.Clear();
            foreach (Student s in students)
            {
                string imagePath = "..\\..\\..\\Database\\Student\\Image\\" + s.Sno + ".jpg";
                Image image = ImageUtil.readFromFile(imagePath);
                if (image != null)
                {
                    imagesFeatureList.Add(ExtractImageFeature(image));
                }
            }
            AppendText(string.Format("已成功提取到{0}个人脸特征!\r\n", imagesFeatureList.Count));
        }
        #endregion

        #region Other
        private IntPtr ExtractImageFeature(Image image)
        {
            if (image == null) return IntPtr.Zero;
            else
            {
                ASF_SingleFaceInfo singleFaceInfo = new ASF_SingleFaceInfo();
                return FaceUtil.ExtractFeature(pImageEngine, image, out singleFaceInfo);
            }
        }

        private void StartVideo()
        {
            //在点击开始的时候再坐下初始化检测，防止程序启动时有摄像头，在点击摄像头按钮之前将摄像头拔掉的情况
            //initVideo();
            //必须保证有可用摄像头
            if (filterInfoCollection.Count == 0)
            {
                MessageBox.Show("未检测到摄像头，请确保已安装摄像头或驱动!");
                return;
            }
            FinishVideo();
            //显示摄像头控件
            rgbVideoSource.Show();
            irVideoSource.Show();
            //获取filterInfoCollection的总数
            int maxCameraCount = filterInfoCollection.Count;
            //如果配置了两个不同的摄像头索引
            if (rgbCameraIndex != irCameraIndex && maxCameraCount >= 2)
            {
                //RGB摄像头加载
                rgbDeviceVideo = new VideoCaptureDevice(filterInfoCollection[rgbCameraIndex < maxCameraCount ? rgbCameraIndex : 0].MonikerString);
                rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[0];
                rgbVideoSource.VideoSource = rgbDeviceVideo;
                rgbVideoSource.Start();

                //IR摄像头
                irDeviceVideo = new VideoCaptureDevice(filterInfoCollection[irCameraIndex < maxCameraCount ? irCameraIndex : 0].MonikerString);
                irDeviceVideo.VideoResolution = irDeviceVideo.VideoCapabilities[0];
                irVideoSource.VideoSource = irDeviceVideo;
                irVideoSource.Start();
                //双摄标志设为true
                isDoubleShot = true;
            }
            else
            {
                //仅打开RGB摄像头，IR摄像头控件隐藏
                rgbDeviceVideo = new VideoCaptureDevice(filterInfoCollection[rgbCameraIndex <= maxCameraCount ? rgbCameraIndex : 0].MonikerString);
                rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[0];
                rgbVideoSource.VideoSource = rgbDeviceVideo;
                rgbVideoSource.Start();
                irVideoSource.Hide();
            }
            AppendText("摄像头已开启!\r\n");
        }

        private void FinishVideo()
        {
            if (irVideoSource.IsRunning)
            {
                irVideoSource.SignalToStop();
                irVideoSource.Hide();
            }
            if (rgbVideoSource.IsRunning)
            {
                rgbVideoSource.SignalToStop();
                rgbVideoSource.Hide();
                AppendText("摄像头已关闭!\r\n");
            }
        }

        private void WriteRecord()
        {
            using (StreamWriter sw = File.CreateText("..\\..\\..\\Database\\Class\\List\\" + classes[comboBoxClass.SelectedIndex].Cno + ".csv"))
            {
                string strLine = "Sno";
                foreach (Record r in records)
                {
                    strLine += "," + r.Rname;
                }
                sw.WriteLine(strLine);
                for (int i = 0; i < students.Count; i++)
                {
                    strLine = students[i].Sno;
                    foreach (Record r in records)
                    {
                        if (r.list[i]) strLine += ",1";
                        else strLine += ",0";
                    }
                    sw.WriteLine(strLine);
                }
            }
            AppendText("已成功保存考勤记录!\r\n");
        }
        #endregion

        #region Change
        private void comboBoxClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            importStudent(comboBoxClass.SelectedIndex);
            importRecord(comboBoxClass.SelectedIndex);
            labelClassCno.Text = "课程号：" + classes[comboBoxClass.SelectedIndex].Cno;
            labelClassCname.Text = "课程名：" + classes[comboBoxClass.SelectedIndex].Cname;
            labelClassCount.Text = "考勤次数：" + classes[comboBoxClass.SelectedIndex].recordNum.ToString();
        }

        private void comboBoxStudent_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelStudentSno.Text = "学号：" + students[comboBoxStudent.SelectedIndex].Sno;
            labelStudentSname.Text = "姓名：" + students[comboBoxStudent.SelectedIndex].Sname;
            labelStudentGender.Text = "性别：" + students[comboBoxStudent.SelectedIndex].Gender;
            if (comboBoxClass.SelectedIndex == 0) labelStudentAttendance.Text = "到课次数：";
            else labelStudentAttendance.Text = "到课次数：" + students[comboBoxStudent.SelectedIndex].Attendance.ToString();
            importImage(students[comboBoxStudent.SelectedIndex].Sno);
            if (records.Count > 0)
            {
                labelRecordName.Text = "姓名：" + students[comboBoxStudent.SelectedIndex].Sname;
                if (records[comboBoxRecord.SelectedIndex].list[comboBoxStudent.SelectedIndex]) labelRecordStatus.Text = "状态：已签到";
                else labelRecordStatus.Text = "状态：未签到";
            }
            else
            {
                labelRecordName.Text = "姓名：";
                labelRecordStatus.Text = "状态：";
            }
        }

        private void comboBoxRecord_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonStart.Enabled = true;
            labelRecordAttendance.Text = "已到课人数：" + records[comboBoxRecord.SelectedIndex].Attendance.ToString();
            labelRecordSkip.Text = "未到课人数：" + (students.Count - records[comboBoxRecord.SelectedIndex].Attendance).ToString();
            if (students.Count > 0)
            {
                labelRecordName.Text = "姓名：" + students[comboBoxStudent.SelectedIndex].Sname;
                if (records[comboBoxRecord.SelectedIndex].list[comboBoxStudent.SelectedIndex]) labelRecordStatus.Text = "状态：已签到";
                else labelRecordStatus.Text = "状态：未签到";
            }
            else
            {
                labelRecordName.Text = "姓名：";
                labelRecordStatus.Text = "状态：";
            }
        }
        #endregion

        #region Click
        private void buttonClassNew_Click(object sender, EventArgs e)
        {
            FormClass formClass = new FormClass();
            if (formClass.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void buttonClassDelete_Click(object sender, EventArgs e)
        {

        }

        private void buttonClassEdit_Click(object sender, EventArgs e)
        {
            FormClass formClass = new FormClass();
            if (formClass.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void buttonStudentNew_Click(object sender, EventArgs e)
        {
            getClass = comboBoxClass.SelectedItem.ToString();
            getStudent = comboBoxStudent.SelectedItem.ToString();
            FormStudent formStudent = new FormStudent();
            if (formStudent.ShowDialog() == DialogResult.OK)
            {

            }
        }
        //判断是否所有课程都没有该学生
        private bool ExistStudent(string sno)
        {
            bool flag = true; //标志位
            int lines = 0; //统计课程数，用来定义数组记录课程号
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
            foreach (string c in Course)
            {
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\List\\" + c + ".csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strClass;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strClass = strLine.Split(',');
                        if (strClass[0] == sno)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag == false)
                        break;
                }
            }
            return flag;
        }

        //在对应课程中删除该学生信息
        private void deleteStudent(string Cno, string Sno)
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
                    if (strArray[0] == Cno)
                        record = Convert.ToInt32(strArray[2]);
                }
            }
            for (int i = 0; i < record; i++)
            {
                string str = (",Record" + (i + 1).ToString());
                text += str;
            }
            text += "\r\n";
            //将课程数据全部读出（除去对应的学号）读入text
            using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Class\\List\\" + Cno + ".csv"))
            {
                string strLine = sr.ReadLine();
                string[] strClass;
                while ((strLine = sr.ReadLine()) != null)
                {
                    strClass = strLine.Split(',');
                    if (strClass[0] == Sno)
                        continue;
                    else
                    {
                        text += strLine + "\r\n";
                    }
                }
            }
            //将text重新写入
            using (StreamWriter writer = new StreamWriter("..\\..\\..\\Database\\Class\\List\\" + Cno + ".csv"))
            {
                writer.Write(text);
            }
        }
        private void buttonStudentDelete_Click(object sender, EventArgs e)
        {
            getClass = comboBoxClass.SelectedItem.ToString();
            string getStudent = comboBoxStudent.SelectedItem.ToString();
            string[] strArray1, strArray2;
            strArray1 = getClass.Split(' ');
            string Cno = strArray1[0], Cname = strArray1[1];
            strArray2 = getStudent.Split(' ');
            string Sno = strArray2[0], Sname = strArray2[1];
            //即删除该学生所有记录
            if (getClass == " ALL STUDENTS")
            {
                //首先删除学生表中学生信息及照片
                int lines = 0;  //统计课程数，用来定义数组记录课程号
                string txt = "Sno,Sname,Gender" + "\r\n";
                using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
                {
                    string strLine = sr.ReadLine();
                    string[] strStudent;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        strStudent = strLine.Split(',');
                        if (strStudent[0] == Sno)
                            continue;
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
                File.Delete("..\\..\\..\\Database\\Student\\Image\\" + Sno + ".jpg");
                //统计课程数目
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
                //在所有课程中寻找该学生信息，若有则删除
                foreach (string c in Course)
                {
                    deleteStudent(c, Sno);
                }
            }
            //删除该学生在对应课程的记录
            else
            {
                deleteStudent(Cno, Sno);
                //如果所有课程都没有该学生，则删除该学生信息
                if (ExistStudent(Sno))
                {
                    string txt = "Sno,Sname,Gender" + "\r\n";
                    using (StreamReader sr = File.OpenText("..\\..\\..\\Database\\Student\\Student.csv"))
                    {
                        string strLine = sr.ReadLine();
                        string[] strClass;
                        while ((strLine = sr.ReadLine()) != null)
                        {
                            strClass = strLine.Split(',');
                            if (strClass[0] == Sno)
                                continue;
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
                    File.Delete("..\\..\\..\\Database\\Student\\Image\\" + Sno + ".jpg");
                }
            }
            MessageBox.Show("删除成功");
        }

        private void buttonStudentEdit_Click(object sender, EventArgs e)
        {
        getClass = comboBoxClass.SelectedItem.ToString();
        getStudent = comboBoxStudent.SelectedItem.ToString();
        FormStudent formStudent = new FormStudent();
            if (formStudent.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void buttonRecordNew_Click(object sender, EventArgs e)
        {
            FormRecord formRecord = new FormRecord();
            if (formRecord.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void buttonRecordDelete_Click(object sender, EventArgs e)
        {

        }

        private void buttonRecordDisplay_Click(object sender, EventArgs e)
        {
            FormDisplay formDisplay = new FormDisplay();
            formDisplay.ShowDialog();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text == "开始考勤")
            {
                buttonStart.Text = "结束考勤";
                buttonClassNew.Enabled = false;
                buttonClassDelete.Enabled = false;
                buttonClassEdit.Enabled = false;
                buttonStudentNew.Enabled = false;
                buttonStudentDelete.Enabled = false;
                buttonStudentEdit.Enabled = false;
                buttonRecordNew.Enabled = false;
                buttonRecordDelete.Enabled = false;
                buttonRecordDisplay.Enabled = false;
                comboBoxClass.Enabled = false;
                comboBoxStudent.Enabled = false;
                comboBoxRecord.Enabled = false;
                textBoxThreshold.Enabled = false;
                importFeature();
                StartVideo();
                float t = float.Parse(textBoxThreshold.Text);
                if (t > 0 && t < 1) threshold = t;

            }
            else if (buttonStart.Text == "结束考勤")
            {
                buttonStart.Text = "开始考勤";
                buttonClassNew.Enabled = true;
                buttonClassDelete.Enabled = true;
                buttonClassEdit.Enabled = true;
                buttonStudentNew.Enabled = true;
                buttonStudentDelete.Enabled = true;
                buttonStudentEdit.Enabled = true;
                buttonRecordNew.Enabled = true;
                buttonRecordDelete.Enabled = true;
                buttonRecordDisplay.Enabled = true;
                comboBoxClass.Enabled = true;
                comboBoxStudent.Enabled = true;
                comboBoxRecord.Enabled = true;
                textBoxThreshold.Enabled = true;
                FinishVideo();
                WriteRecord();

            }
            else
            {
                AppendText("按钮异常!\n\n");
            }
        }
        #endregion

        #region Close
        private void AuthenticationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                FinishVideo();
                //销毁引擎
                int retCode = ASFFunctions.ASFUninitEngine(pImageEngine);
                Console.WriteLine("UninitEngine pImageEngine Result:" + retCode);
                //销毁引擎
                retCode = ASFFunctions.ASFUninitEngine(pVideoEngine);
                Console.WriteLine("UninitEngine pVideoEngine Result:" + retCode);

                //销毁引擎
                retCode = ASFFunctions.ASFUninitEngine(pVideoRGBImageEngine);
                Console.WriteLine("UninitEngine pVideoImageEngine Result:" + retCode);

                //销毁引擎
                retCode = ASFFunctions.ASFUninitEngine(pVideoIRImageEngine);
                Console.WriteLine("UninitEngine pVideoIRImageEngine Result:" + retCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UninitEngine pImageEngine Error:" + ex.Message);
            }
        }
        #endregion

        #region Video
        private FaceTrackUnit trackRGBUnit = new FaceTrackUnit();
        private FaceTrackUnit trackIRUnit = new FaceTrackUnit();
        private Font font = new Font(FontFamily.GenericSerif, 10f, FontStyle.Bold);
        private SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
        private SolidBrush blueBrush = new SolidBrush(Color.Blue);
        private bool isRGBLock = false;
        private bool isIRLock = false;
        private MRECT allRect = new MRECT();
        private object rectLock = new object();

        private void rgbVideoSource_Paint(object sender, PaintEventArgs e)
        {
            if (rgbVideoSource.IsRunning)
            {
                //得到当前RGB摄像头下的图片
                Bitmap bitmap = rgbVideoSource.GetCurrentVideoFrame();
                if (bitmap == null)
                {
                    return;
                }
                //检测人脸，得到Rect框
                ASF_MultiFaceInfo multiFaceInfo = FaceUtil.DetectFace(pVideoEngine, bitmap);
                //得到最大人脸
                ASF_SingleFaceInfo maxFace = FaceUtil.GetMaxFace(multiFaceInfo);
                //得到Rect
                MRECT rect = maxFace.faceRect;
                //检测RGB摄像头下最大人脸
                Graphics g = e.Graphics;
                float offsetX = rgbVideoSource.Width * 1f / bitmap.Width;
                float offsetY = rgbVideoSource.Height * 1f / bitmap.Height;
                float x = rect.left * offsetX;
                float width = rect.right * offsetX - x;
                float y = rect.top * offsetY;
                float height = rect.bottom * offsetY - y;
                //根据Rect进行画框
                g.DrawRectangle(Pens.Red, x, y, width, height);
                if (trackRGBUnit.message != "" && x > 0 && y > 0)
                {
                    //将上一帧检测结果显示到页面上
                    g.DrawString(trackRGBUnit.message, font, trackRGBUnit.message.Contains("活体") ? blueBrush : yellowBrush, x, y - 15);
                }

                //保证只检测一帧，防止页面卡顿以及出现其他内存被占用情况
                if (isRGBLock == false)
                {
                    isRGBLock = true;
                    //异步处理提取特征值和比对，不然页面会比较卡
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                    {
                        if (rect.left != 0 && rect.right != 0 && rect.top != 0 && rect.bottom != 0)
                        {
                            try
                            {
                                lock (rectLock)
                                {
                                    allRect.left = (int)(rect.left * offsetX);
                                    allRect.top = (int)(rect.top * offsetY);
                                    allRect.right = (int)(rect.right * offsetX);
                                    allRect.bottom = (int)(rect.bottom * offsetY);
                                }

                                bool isLiveness = false;

                                //调整图片数据，非常重要
                                ImageInfo imageInfo = ImageUtil.ReadBMP(bitmap);
                                if (imageInfo == null)
                                {
                                    return;
                                }
                                int retCode_Liveness = -1;
                                //RGB活体检测
                                ASF_LivenessInfo liveInfo = FaceUtil.LivenessInfo_RGB(pVideoRGBImageEngine, imageInfo, multiFaceInfo, out retCode_Liveness);
                                //判断检测结果
                                if (retCode_Liveness == 0 && liveInfo.num > 0)
                                {
                                    int isLive = MemoryUtil.PtrToStructure<int>(liveInfo.isLive);
                                    isLiveness = (isLive == 1) ? true : false;
                                }
                                if (imageInfo != null)
                                {
                                    MemoryUtil.Free(imageInfo.imgData);
                                }
                                if (isLiveness)
                                {
                                    //提取人脸特征
                                    IntPtr feature = FaceUtil.ExtractFeature(pVideoRGBImageEngine, bitmap, maxFace);
                                    float similarity = 0f;
                                    //得到比对结果
                                    int result = compareFeature(feature, out similarity);
                                    MemoryUtil.Free(feature);
                                    if (result > -1)
                                    {
                                        //将比对结果放到显示消息中，用于最新显示
                                        trackRGBUnit.message = string.Format(" {0} {1},{2}", students[result].Sname, similarity, string.Format("RGB{0}", isLiveness ? "活体" : "假体"));
                                        labelRecordName.Text = "姓名：" + students[result].Sname;
                                        if (similarity >= threshold && records[comboBoxRecord.SelectedIndex].list[result] == false)
                                        {
                                            records[comboBoxRecord.SelectedIndex].list[result] = true;
                                            records[comboBoxRecord.SelectedIndex].Attendance++;
                                            students[result].Attendance++;
                                            AppendText(string.Format("{0}成功签到!\r\n", students[result].Sname));
                                            labelRecordAttendance.Text = "已到课人数：" + records[comboBoxRecord.SelectedIndex].Attendance.ToString();
                                            labelRecordSkip.Text = "未到课人数：" + (students.Count - records[comboBoxRecord.SelectedIndex].Attendance).ToString();
                                            if (result == comboBoxStudent.SelectedIndex)
                                            {
                                                labelStudentAttendance.Text = "到课次数：" + students[result].Attendance.ToString();
                                            }
                                        }
                                        if (records[comboBoxRecord.SelectedIndex].list[result]) labelRecordStatus.Text = "状态：已签到";
                                        else labelRecordStatus.Text = "状态：未签到";
                                    }
                                    else
                                    {
                                        //显示消息
                                        trackRGBUnit.message = string.Format("RGB{0}", isLiveness ? "活体" : "假体");
                                        labelRecordName.Text = "姓名：";
                                        labelRecordStatus.Text = "状态：";
                                    }
                                }
                                else
                                {
                                    //显示消息
                                    trackRGBUnit.message = string.Format("RGB{0}", isLiveness ? "活体" : "假体");
                                    labelRecordName.Text = "姓名：";
                                    labelRecordStatus.Text = "状态：";
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                if (bitmap != null)
                                {
                                    bitmap.Dispose();
                                }
                                isRGBLock = false;
                            }
                        }
                        else
                        {
                            lock (rectLock)
                            {
                                allRect.left = 0;
                                allRect.top = 0;
                                allRect.right = 0;
                                allRect.bottom = 0;
                            }
                        }
                        isRGBLock = false;
                    }));
                }
            }
        }

        private void irVideoSource_Paint(object sender, PaintEventArgs e)
        {
            if (isDoubleShot && irVideoSource.IsRunning)
            {
                //如果双摄，且IR摄像头工作，获取IR摄像头图片
                Bitmap irBitmap = irVideoSource.GetCurrentVideoFrame();
                if (irBitmap == null)
                {
                    return;
                }
                //得到Rect
                MRECT rect = new MRECT();
                lock (rectLock)
                {
                    rect = allRect;
                }
                float irOffsetX = irVideoSource.Width * 1f / irBitmap.Width;
                float irOffsetY = irVideoSource.Height * 1f / irBitmap.Height;
                float offsetX = irVideoSource.Width * 1f / rgbVideoSource.Width;
                float offsetY = irVideoSource.Height * 1f / rgbVideoSource.Height;
                //检测IR摄像头下最大人脸
                Graphics g = e.Graphics;

                float x = rect.left * offsetX;
                float width = rect.right * offsetX - x;
                float y = rect.top * offsetY;
                float height = rect.bottom * offsetY - y;
                //根据Rect进行画框
                g.DrawRectangle(Pens.Red, x, y, width, height);
                if (trackIRUnit.message != "" && x > 0 && y > 0)
                {
                    //将上一帧检测结果显示到页面上
                    g.DrawString(trackIRUnit.message, font, trackIRUnit.message.Contains("活体") ? blueBrush : yellowBrush, x, y - 15);
                }

                //保证只检测一帧，防止页面卡顿以及出现其他内存被占用情况
                if (isIRLock == false)
                {
                    isIRLock = true;
                    //异步处理提取特征值和比对，不然页面会比较卡
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
                    {
                        if (rect.left != 0 && rect.right != 0 && rect.top != 0 && rect.bottom != 0)
                        {
                            bool isLiveness = false;
                            try
                            {
                                //得到当前摄像头下的图片
                                if (irBitmap != null)
                                {
                                    //检测人脸，得到Rect框
                                    ASF_MultiFaceInfo irMultiFaceInfo = FaceUtil.DetectFace(pVideoIRImageEngine, irBitmap);
                                    if (irMultiFaceInfo.faceNum <= 0)
                                    {
                                        return;
                                    }
                                    //得到最大人脸
                                    ASF_SingleFaceInfo irMaxFace = FaceUtil.GetMaxFace(irMultiFaceInfo);
                                    //得到Rect
                                    MRECT irRect = irMaxFace.faceRect;
                                    //判断RGB图片检测的人脸框与IR摄像头检测的人脸框偏移量是否在误差允许范围内
                                    if (isInAllowErrorRange(rect.left * offsetX / irOffsetX, irRect.left) && isInAllowErrorRange(rect.right * offsetX / irOffsetX, irRect.right)
                                        && isInAllowErrorRange(rect.top * offsetY / irOffsetY, irRect.top) && isInAllowErrorRange(rect.bottom * offsetY / irOffsetY, irRect.bottom))
                                    {
                                        int retCode_Liveness = -1;
                                        //将图片进行灰度转换，然后获取图片数据
                                        ImageInfo irImageInfo = ImageUtil.ReadBMP_IR(irBitmap);
                                        if (irImageInfo == null)
                                        {
                                            return;
                                        }
                                        //IR活体检测
                                        ASF_LivenessInfo liveInfo = FaceUtil.LivenessInfo_IR(pVideoIRImageEngine, irImageInfo, irMultiFaceInfo, out retCode_Liveness);
                                        //判断检测结果
                                        if (retCode_Liveness == 0 && liveInfo.num > 0)
                                        {
                                            int isLive = MemoryUtil.PtrToStructure<int>(liveInfo.isLive);
                                            isLiveness = (isLive == 1) ? true : false;
                                        }
                                        if (irImageInfo != null)
                                        {
                                            MemoryUtil.Free(irImageInfo.imgData);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                trackIRUnit.message = string.Format("IR{0}", isLiveness ? "活体" : "假体");
                                if (irBitmap != null)
                                {
                                    irBitmap.Dispose();
                                }
                                isIRLock = false;
                            }
                        }
                        else
                        {
                            trackIRUnit.message = string.Empty;
                        }
                        isIRLock = false;
                    }));
                }
            }
        }

        private int compareFeature(IntPtr feature, out float similarity_max)
        {
            int result = -1;
            similarity_max = 0f;
            float similarity = 0f;
            //如果人脸库不为空，则进行人脸匹配
            if (imagesFeatureList != null && imagesFeatureList.Count > 0)
            {
                for (int i = 0; i < imagesFeatureList.Count; i++)
                {
                    //调用人脸匹配方法，进行匹配
                    ASFFunctions.ASFFaceFeatureCompare(pVideoRGBImageEngine, feature, imagesFeatureList[i], ref similarity);
                    if (similarity > similarity_max)
                    {
                        result = i;
                        similarity_max = similarity;
                    }
                }
            }
            return result;
        }

        private void videoSource_PlayingFinished(object sender, AForge.Video.ReasonToFinishPlaying reason)
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;
                //chooseImgBtn.Enabled = true;
                //matchBtn.Enabled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 参数定义
        /// <summary>
        /// 引擎Handle
        /// </summary>
        private IntPtr pImageEngine = IntPtr.Zero;

        /// <summary>
        /// 保存右侧图片路径
        /// </summary>
        private string image1Path;

        /// <summary>
        /// 图片最大大小
        /// </summary>
        private long maxSize = 1024 * 1024 * 2;

        /// <summary>
        /// 右侧图片人脸特征
        /// </summary>
        private IntPtr image1Feature;

        /// <summary>
        /// 保存对比图片的列表
        /// </summary>
        private List<string> imagePathList = new List<string>();

        /// <summary>
        /// 左侧图库人脸特征列表
        /// </summary>
        private List<IntPtr> imagesFeatureList = new List<IntPtr>();

        /// <summary>
        /// 相似度
        /// </summary>
        private float threshold = 0.8f;

        /// <summary>
        /// 用于标记是否需要清除比对结果
        /// </summary>
        private bool isCompare = false;

        /// <summary>
        /// 是否是双目摄像
        /// </summary>
        private bool isDoubleShot = false;

        /// <summary>
        /// 允许误差范围
        /// </summary>
        private int allowAbleErrorRange = 40;

        /// <summary>
        /// RGB 摄像头索引
        /// </summary>
        private int rgbCameraIndex = 0;

        /// <summary>
        /// IR 摄像头索引
        /// </summary>
        private int irCameraIndex = 0;

        /// <summary>
        /// 记录选中的课程
        /// </summary>
        public static string getClass;
        /// <summary>
        /// 记录选中的学生
        /// </summary>
        public static string getStudent;
        #region 视频模式下相关
        /// <summary>
        /// 视频引擎Handle
        /// </summary>
        private IntPtr pVideoEngine = IntPtr.Zero;
        /// <summary>
        /// RGB视频引擎 FR Handle 处理   FR和图片引擎分开，减少强占引擎的问题
        /// </summary>
        private IntPtr pVideoRGBImageEngine = IntPtr.Zero;
        /// <summary>
        /// IR视频引擎 FR Handle 处理   FR和图片引擎分开，减少强占引擎的问题
        /// </summary>
        private IntPtr pVideoIRImageEngine = IntPtr.Zero;
        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        private FilterInfoCollection filterInfoCollection;
        /// <summary>
        /// RGB摄像头设备
        /// </summary>
        private VideoCaptureDevice rgbDeviceVideo;
        /// <summary>
        /// IR摄像头设备
        /// </summary>
        private VideoCaptureDevice irDeviceVideo;
        #endregion
        #endregion

        #region 初始化

        /// <summary>
        /// 初始化引擎
        /// </summary>
        private void InitEngines()
        {
            //读取配置文件
            AppSettingsReader reader = new AppSettingsReader();
            string appId = (string)reader.GetValue("APP_ID", typeof(string));
            string sdkKey64 = (string)reader.GetValue("SDKKEY64", typeof(string));
            string sdkKey32 = (string)reader.GetValue("SDKKEY32", typeof(string));
            rgbCameraIndex = (int)reader.GetValue("RGB_CAMERA_INDEX", typeof(int));
            irCameraIndex = (int)reader.GetValue("IR_CAMERA_INDEX", typeof(int));
            //判断CPU位数
            var is64CPU = Environment.Is64BitProcess;
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(is64CPU ? sdkKey64 : sdkKey32))
            {
                //禁用相关功能按钮
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                MessageBox.Show(string.Format("请在App.config配置文件中先配置APP_ID和SDKKEY{0}!", is64CPU ? "64" : "32"));
                return;
            }

            //在线激活引擎    如出现错误，1.请先确认从官网下载的sdk库已放到对应的bin中，2.当前选择的CPU为x86或者x64
            int retCode = 0;
            try
            {
                retCode = ASFFunctions.ASFActivation(appId, is64CPU ? sdkKey64 : sdkKey32);
            }
            catch (Exception ex)
            {
                //禁用相关功能按钮
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
                if (ex.Message.Contains("无法加载 DLL"))
                {
                    MessageBox.Show("请将sdk相关DLL放入bin对应的x86或x64下的文件夹中!");
                }
                else
                {
                    MessageBox.Show("激活引擎失败!");
                }
                return;
            }
            Console.WriteLine("Activate Result:" + retCode);

            //初始化引擎
            uint detectMode = DetectionMode.ASF_DETECT_MODE_IMAGE;
            //Video模式下检测脸部的角度优先值
            int videoDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_0_HIGHER_EXT;
            //Image模式下检测脸部的角度优先值
            int imageDetectFaceOrientPriority = ASF_OrientPriority.ASF_OP_0_ONLY;
            //人脸在图片中所占比例，如果需要调整检测人脸尺寸请修改此值，有效数值为2-32
            int detectFaceScaleVal = 16;
            //最大需要检测的人脸个数
            int detectFaceMaxNum = 5;
            //引擎初始化时需要初始化的检测功能组合
            int combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_AGE | FaceEngineMask.ASF_GENDER | FaceEngineMask.ASF_FACE3DANGLE;
            //初始化引擎，正常值为0，其他返回值请参考http://ai.arcsoft.com.cn/bbs/forum.php?mod=viewthread&tid=19&_dsign=dbad527e
            retCode = ASFFunctions.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pImageEngine);
            Console.WriteLine("InitEngine Result:" + retCode);
            AppendText((retCode == 0) ? "引擎初始化成功!\r\n" : string.Format("引擎初始化失败!错误码为:{0}\r\n", retCode));
            if (retCode != 0)
            {
                //禁用相关功能按钮
                //ControlsEnable(false, chooseMultiImgBtn, matchBtn, btnClearFaceList, chooseImgBtn);
            }

            //初始化视频模式下人脸检测引擎
            uint detectModeVideo = DetectionMode.ASF_DETECT_MODE_VIDEO;
            int combinedMaskVideo = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION;
            retCode = ASFFunctions.ASFInitEngine(detectModeVideo, videoDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMaskVideo, ref pVideoEngine);
            //RGB视频专用FR引擎
            detectFaceMaxNum = 1;
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_LIVENESS;
            retCode = ASFFunctions.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pVideoRGBImageEngine);

            //IR视频专用FR引擎
            combinedMask = FaceEngineMask.ASF_FACE_DETECT | FaceEngineMask.ASF_FACERECOGNITION | FaceEngineMask.ASF_IR_LIVENESS;
            retCode = ASFFunctions.ASFInitEngine(detectMode, imageDetectFaceOrientPriority, detectFaceScaleVal, detectFaceMaxNum, combinedMask, ref pVideoIRImageEngine);

            Console.WriteLine("InitVideoEngine Result:" + retCode);

            initVideo();
        }

        /// <summary>
        /// 摄像头初始化
        /// </summary>
        private void initVideo()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //如果没有可用摄像头，“启用摄像头”按钮禁用，否则使可用
            if (filterInfoCollection.Count == 0)
            {
                //buttonStart.Enabled = false;
                AppendText("摄像头不可用!\r\n");
            }
            else
            {
                //buttonStart.Enabled = true;
                AppendText("摄像头可用!\r\n");
            }
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 恢复使用/禁用控件列表控件
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="controls">控件列表</param>
        private void ControlsEnable(bool isEnable, params Control[] controls)
        {
            if (controls == null || controls.Length <= 0)
            {
                return;
            }
            foreach (Control control in controls)
            {
                control.Enabled = isEnable;
            }
        }

        /// <summary>
        /// 校验图片
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private bool checkImage(string imagePath)
        {
            if (imagePath == null)
            {
                AppendText("图片不存在，请确认后再导入\r\n");
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
                AppendText(string.Format("{0} 图片格式有问题，请确认后再导入\r\n", imagePath));
                return false;
            }
            FileInfo fileCheck = new FileInfo(imagePath);
            if (fileCheck.Exists == false)
            {
                AppendText(string.Format("{0} 不存在\r\n", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length > maxSize)
            {
                AppendText(string.Format("{0} 图片大小超过2M，请压缩后再导入\r\n", fileCheck.Name));
                return false;
            }
            else if (fileCheck.Length < 2)
            {
                AppendText(string.Format("{0} 图像质量太小，请重新选择\r\n", fileCheck.Name));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 追加公用方法
        /// </summary>
        /// <param name="message"></param>
        private void AppendText(string message)
        {
            logBox.AppendText(message);
        }

        /// <summary>
        /// 判断参数0与参数1是否在误差允许范围内
        /// </summary>
        /// <param name="arg0">参数0</param>
        /// <param name="arg1">参数1</param>
        /// <returns></returns>
        private bool isInAllowErrorRange(float arg0, float arg1)
        {
            bool rel = false;
            if (arg0 > arg1 - allowAbleErrorRange && arg0 < arg1 + allowAbleErrorRange)
            {
                rel = true;
            }
            return rel;
        }
        #endregion
    }
}
