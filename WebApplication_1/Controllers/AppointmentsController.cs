using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication_1.Models;

namespace WebApplication_1.Controllers
{
    public class AppointmentsController : Controller
    {
        private HospitalContext db = new HospitalContext();

        // GET: Appointments
        public ActionResult Index()
        {
            var appointment = db.appointment.Include(a => a.doc).Include(a => a.pat);
            return View(appointment.ToList());
        }

        // GET: Appointments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.appointment.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // GET: Appointments/Create
        public ActionResult Create()
        {

            List<Times> tlist = new List<Times>();
            var specList = db.doctor.Select(e => new
            {
                id = e.speciality,
                name = e.speciality
            });
 
            ViewBag.doc_id = new SelectList(db.doctor, "id", "doctor_name");
            ViewBag.pat_id = new SelectList(db.patient, "id", "patient_name");
            ViewBag.appointTime = new SelectList(tlist, "val", "txt");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        public JsonResult FetchSchedule(DateTime date, int docid)
        {
            string DOW = date.ToString("ddd");
            string schedule = db.schedule.Single(e => e.doc_id == docid).sch;
            string[] sch = schedule.Split(',');
            string docSchedule = "";
            foreach (string str in sch) //Mon-9_18
            {
                string[] str1 = str.Split('-');
                if (str1[0] == DOW)
                {
                    docSchedule = str1[1];
                    break;
                }
            }
            string[] startend = docSchedule.Split('_');
            int start = int.Parse(startend[0]);
            int end = int.Parse(startend[1]);
            List<Times> tlist = new List<Times>();
            List<Appointment> booked = db.appointment.Where(e => e.doc_id == docid && e.date == date).ToList();
            int[] time2 = new int[booked.Count];
            int count = 0;
            foreach (Appointment a in booked)
            {
                time2[count] = a.appointTime;
                count++;
            }
            for (int i = 0; i < end - start; i++)
            {
                Times t = new Times();
                if (!time2.Contains(start + i))
                {
                    t.txt = start + i;
                    t.val = start + i;
                    tlist.Add(t);
                }
            }
            var tms = tlist.Select(e => new
            {
                txt = e.txt,
                val = e.val
            });
            return Json(tms, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,pat_id,doc_id,date,appointTime")] Appointment appointment)
        {
            List<Times> tlist = new List<Times>();
            if (ModelState.IsValid)
            {
                db.appointment.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.doc_id = new SelectList(db.doctor, "id", "doctor_name", appointment.doc_id);
            ViewBag.pat_id = new SelectList(db.patient, "id", "patient_name", appointment.pat_id);
            ViewBag.appointTime = new SelectList(tlist, "val", "txt");
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.appointment.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            ViewBag.doc_id = new SelectList(db.doctor, "id", "doctor_name", appointment.doc_id);
            ViewBag.pat_id = new SelectList(db.patient, "id", "patient_name", appointment.pat_id);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,pat_id,doc_id,date,appointTime")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.doc_id = new SelectList(db.doctor, "id", "doctor_name", appointment.doc_id);
            ViewBag.pat_id = new SelectList(db.patient, "id", "patient_name", appointment.pat_id);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.appointment.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.appointment.Find(id);
            db.appointment.Remove(appointment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
