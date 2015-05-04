﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using _360Feedback.Models;
using _360Feedback.DataContexts;

namespace _360Feedback.Controllers
{
    public class HomeController : Controller
    {
        private FeedbackDb Db = new FeedbackDb();

        public ActionResult Index()
        {
            List<Team> teams = Db.Teams.ToList<Team>();

            return View(teams);
        }

        
        [HttpPost]
        public async Task<ActionResult> SaveNewTeam()
        {
            Team newTeam = new Team();
            List<Student> studentList = new List<Student>();
            newTeam.TeamName = Request.Params["teamName"];
            int studentCount = Int32.Parse(Request.Params["counter"]);
            for(int i = 0; i < studentCount;i++)
            {
                Student addStudent = new Student();
                addStudent.Name = Request.Params["student" + i.ToString()];
                addStudent.Email = Request.Params["email" + i.ToString()];
                addStudent.Completed = false;
                addStudent.Team = newTeam;
                studentList.Add(addStudent);
            }
            Db.Teams.Add(newTeam);
            await Db.SaveChangesAsync();
            foreach(Student s in studentList)
            {
                Db.Students.Add(s);
            }
            await Db.SaveChangesAsync();
            return Redirect("Index");
        }

        [HttpPost]
        public async Task<ActionResult> DeleteTeam()
        {
            Team deleteTeam = Db.Teams.Find(Int32.Parse(Request.Params["teamId"]));

            Db.Teams.Remove(deleteTeam);
            await Db.SaveChangesAsync();
            return Redirect("Index");
        }
        
        [HttpPost]
        public ActionResult EditTeam()
        {
            Team editTeam = Db.Teams.Find(Int32.Parse(Request.Params["teamId"]));

            return View("EditTeam", editTeam);
        }

        [HttpPost]
        public async Task<ActionResult> SaveTeam()
        {
            Team editTeam = Db.Teams.Find(Int32.Parse(Request.Params["teamId"]));
            List<Student> studentList = editTeam.Students.ToList();
            editTeam.TeamName = Request.Params["teamName"];
            int studentCount = Int32.Parse(Request.Params["counter"]);
            for (int i = 0; i <= studentCount; i++)
            {
                if (!(Request.Params["email" + i.ToString()] == null) && !(Request.Params["student" + i.ToString()] == null))
                {
                    if(!(Db.Students.Find(Request.Params["email" + i.ToString()]) == null)){
                        Student editStudent = Db.Students.Find(Request.Params["email" + i.ToString()]);
                        editStudent.Name = Request.Params["student" + i.ToString()];
                        editStudent.Email = Request.Params["email" + i.ToString()];
                        await Db.SaveChangesAsync();
                    } else {
                        Student saveStudent = new Student();
                        saveStudent.Name = Request.Params["student" + i.ToString()];
                        saveStudent.Email = Request.Params["email" + i.ToString()];
                        saveStudent.Completed = false;
                        saveStudent.Team = editTeam;
                        studentList.Add(saveStudent);
                        Db.Students.Add(saveStudent);
                        await Db.SaveChangesAsync();
                    }
                    
                }
            }
            await Db.SaveChangesAsync();

            return Redirect("Index");
        }
        
        [HttpPost]
        public async Task<ActionResult> RemoveStudent(String studentEmail)
        {
            Team editTeam = Db.Teams.Find(Int32.Parse(Request.Params["teamId"]));
            Student removeStudent = Db.Students.Find(Request.Params["emailDelete"]);
            editTeam.Students.Remove(removeStudent);
            Db.Students.Remove(removeStudent);
            await Db.SaveChangesAsync();

            return View("EditTeam", editTeam);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View("EmailIndex.cshtml");
        }

        [HttpPost]
        public ActionResult SendEmailToTeam()
        {
            if (ModelState.IsValid)
            {
                Team team = Db.Teams.Find(Int32.Parse(Request.Params["teamId"]));

                foreach (Student student in team.Students)
                {

                    try
                    {
                        MailMessage mail = new MailMessage();
                        mail.To.Add(student.Email);
                        mail.From = new MailAddress("wctcemailtest@gmail.com");
                        // mail.From = new MailAddress("MGreen14@wctc.edu");
                        mail.Subject = "ISP Team Review";
                            string Body = GenerateEmailBody(student);
                        mail.Body = Body;
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        //smtp.Host = "smtp.office365.com";
                        smtp.Port = 587;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential("wctcemailtest@gmail.com", "blackrose7");
                        // smtp.Credentials = new System.Net.NetworkCredential("MGreen14@wctc.edu", "PASSWORD");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);

                        TempData[student.Name] = "- Student E-mail Sent";
                    }
                    catch (SmtpException e)
                    {
                        TempData[student.Name] = "- Error Sending E-mail";
                        return RedirectToAction("Index");
                    }

                }
                
                return RedirectToAction("Index");

            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult SendEmailToStudent()
        {
            if (ModelState.IsValid)
            {
                Student student = Db.Students.Find(Request.Params["studentEmail"]);

                try
                {
                MailMessage mail = new MailMessage();
                    mail.To.Add(student.Email);
                mail.From = new MailAddress("wctcemailtest@gmail.com");
                // mail.From = new MailAddress("MGreen14@wctc.edu");
                mail.Subject = "ISP Team Review";
                    string Body = GenerateEmailBody(student);
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                //smtp.Host = "smtp.office365.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("wctcemailtest@gmail.com", "blackrose7");
                // smtp.Credentials = new System.Net.NetworkCredential("MGreen14@wctc.edu", "PASSWORD");
                smtp.EnableSsl = true;
                smtp.Send(mail);
                TempData[student.Name] = "- Student E-mail Sent";
                return RedirectToAction("Index");
                }
                catch (SmtpException e)
                {
                    TempData[student.Name] = "- Error Sending E-mail";
                    return RedirectToAction("Index");
                }

            }
            else
            {
                return View();
            }

        }

        public string GenerateEmailBody(Student _student)
        {
            Byte[] encoded = GetBytes(_student.Email);
            String base64userName = HttpServerUtility.UrlTokenEncode(encoded);

            
            String body = "You have been invited to complete a survery on your teammates, please follow the link below to complete the survey: \n";
            String url = "http://www.studentteamfeedback.com/Student/StudentView?id=" + base64userName;
            body += url;
            return body;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}