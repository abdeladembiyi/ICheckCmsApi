﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iCheckAPI.Models;

namespace iCheckAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckListRefsController : ControllerBase
    {
        private readonly icheckcmsContext _context;

        public CheckListRefsController(icheckcmsContext context)
        {
            _context = context;
        }

        // GET: api/CheckListRefs
        [HttpGet]
        public async Task<IEnumerable<Object>> GetCheckListRef()
        {
            return await _context.CheckListRef.Select(s => new
            {
                s.Id,
                s.IdCheckListRef,
                s.IdConducteur,
                s.IdVehicule,
                s.IdSite,
                s.IdConducteurNavigation.NomComplet,
                s.IdConducteurNavigation.IdPermisNavigation.Libelle,
                s.IdVehiculeNavigation.Matricule,
                s.IdVehiculeNavigation.IdEnginNavigation.NomEngin,
                s.Date,
                s.Etat,
                s.Rating
            }).ToListAsync();
        }

        [HttpGet("blocked")]
        public IEnumerable<CheckListRef> GetCheckListRefBlocked()
        {
            return _context.CheckListRef.Include(v => v.IdVehiculeNavigation.IdEnginNavigation)
                                        .Include(c => c.IdConducteurNavigation)
                                        .Include(s => s.IdSiteNavigation)
                                        .Where(x => x.Etat == true)
                                        .ToList();
        }

        // GET: api/CheckListRefs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCheckListRef([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var checkListRef = await _context.CheckListRef.FindAsync(id);

            if (checkListRef == null)
            {
                return NotFound();
            }

            return Ok(checkListRef);
        }

        // PUT: api/CheckListRefs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCheckListRef([FromRoute] int id, [FromBody] CheckListRef checkListRef)
        {
            Blockage blockage = new Blockage();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != checkListRef.Id)
            {
                return BadRequest();
            }

            _context.Entry(checkListRef).State = EntityState.Modified;


            blockage.IdVehicule = checkListRef.IdVehicule;
            blockage.DateBlockage = checkListRef.Date.Value.Date;
            blockage.IdCheckList = checkListRef.IdCheckListRef;

            if (!(bool)checkListRef.Etat)
            {

                // _context.Entry(blockage).State = EntityState.Modified;
                var blockageUpdated = await _context.Blockage.FirstOrDefaultAsync(x => x.IdCheckList == blockage.IdCheckList);
                if (blockageUpdated != null)
                {
                    blockageUpdated.DateDeblockage = DateTime.Now.Date;
                    // blockageUpdated = blockage;
                    System.Diagnostics.Debug.WriteLine("Updated: " + blockageUpdated.IdCheckList + ", " + blockageUpdated.Id);
                }
            }
            else
            {
                if ((bool)checkListRef.Etat)
                {
                    _context.Blockage.Add(blockage);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckListRefExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CheckListRefs
        [HttpPost]
        public async Task<IActionResult> PostCheckListRef([FromBody] CheckListRef checkListRef)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.CheckListRef.Add(checkListRef);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCheckListRef", new { id = checkListRef.Id }, checkListRef);
        }

        // DELETE: api/CheckListRefs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCheckListRef([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var checkListRef = await _context.CheckListRef.FindAsync(id);
            if (checkListRef == null)
            {
                return NotFound();
            }

            _context.CheckListRef.Remove(checkListRef);
            await _context.SaveChangesAsync();

            return Ok(checkListRef);
        }

        private bool CheckListRefExists(int id)
        {
            return _context.CheckListRef.Any(e => e.Id == id);
        }
    }
}