using Iboss.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iboss.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly rakibContext _context;

        public EmployeeController(rakibContext context)
        {
            _context = context;
        }

        // API01: Update an employee’s Employee Name and Code [Don't allow duplicate employee code]
        [HttpPut("UpdateEmployee/{employeeId}")]
        public IActionResult UpdateEmployee(int employeeId, [FromBody] tblEmployee updateDto)
        {
            var employee = _context.tblEmployees.FirstOrDefault(e => e.employeeId == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            // Check if the new code already exists
            if (_context.tblEmployees.Any(e => e.employeeCode == updateDto.employeeCode && e.employeeId != employeeId))
            {
                return BadRequest("Employee code already exists.");
            }

            // Update the employee's name and code
            employee.employeeName = updateDto.employeeName;
            employee.employeeCode = updateDto.employeeCode;

            _context.SaveChanges();

            return Ok(employee);
        }


        // API02: Get employee who has 3rd highest salary
        [HttpGet("GetEmployeeWith3rdHighestSalary")]
        public IActionResult GetEmployeeWith3rdHighestSalary()
        {
            var thirdHighestSalary = _context.tblEmployees.OrderByDescending(e => e.employeeSalary)
                                                        .Skip(2).Take(1)
                                                        .FirstOrDefault();

           

            if (thirdHighestSalary == null)
            {
                return NotFound();
            }

            return Ok(thirdHighestSalary);
        }


        // API03: Get all employees based on maximum to minimum salary who have not any absent record
        [HttpGet("GetEmployeesWithNoAbsentRecord")]
        public IActionResult GetEmployeesWithNoAbsentRecord()
        {
            var employeesWithNoAbsent = _context.tblEmployees
                                                 .Where(e => !_context.tblEmployeeAttendances.Any(a => a.employeeId == e.employeeId && (a.isAbsent == null || a.isAbsent == false)))
                                                 .ToList();

            return Ok(employeesWithNoAbsent);
        }


        // API04: Get monthly attendance report of all employees
        [HttpGet("GetMonthlyAttendanceReport")]
        public IActionResult GetMonthlyAttendanceReport()
        {
            var report = _context.tblEmployees
                               .Select(e => new
                               {
                                   EmployeeName = e.employeeName,
                                   MonthName = "June", // Implement logic to get the month name.
                                   PayableSalary = e.employeeSalary,
                                   TotalPresent = _context.tblEmployeeAttendances
                                                        .Where(a => a.employeeId == e.employeeId && (a.isPresent == null || a.isPresent == false))
                                                        .Count(),
                                   TotalAbsent = _context.tblEmployeeAttendances
                                                       .Where(a => a.employeeId == e.employeeId && (a.isAbsent == null || a.isAbsent == false))
                                                       .Count(),
                                   TotalOffday = _context.tblEmployeeAttendances
                                                       .Where(a => a.employeeId == e.employeeId && (a.isOffday == null || a.isOffday == false))
                                                       .Count()
                               })
                               .ToList();

            return Ok(report);
        }


        // API05: Get a hierarchy from an employee based on his supervisor
        [HttpGet("GetEmployeeHierarchy/{employeeId}")]
        public IActionResult GetEmployeeHierarchy(int employeeId)
        {
            List<string> hierarchy = new List<string>();
            HashSet<int> visitedEmployees = new HashSet<int>(); // To keep track of visited employees.

            var currentEmployee = _context.tblEmployees.Find(employeeId);

            while (currentEmployee != null && !visitedEmployees.Contains(currentEmployee.employeeId))
            {
                visitedEmployees.Add(currentEmployee.employeeId); // Mark the employee as visited.
                hierarchy.Add(currentEmployee.employeeName);
                currentEmployee = _context.tblEmployees.Find(currentEmployee.supervisorId);
            }

            hierarchy.Reverse(); // Reverse the list to show the hierarchy from top to bottom.

            return Ok(hierarchy);
        }

    }
}
