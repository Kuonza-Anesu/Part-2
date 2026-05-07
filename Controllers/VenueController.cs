using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor injects both the database context and configuration settings
        public VenueController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        public IActionResult Create()
        {
            // Make sure you use a list from the context
            ViewBag.Venues = _context.Venue.ToList();
            return View();
        }

        // ============================
        // Save new venue
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                // If an image file was uploaded, store it in Azurite Blob Storage
                if (venue.ImageFile != null)
                {
                    // Upload image and save returned blob URL into ImageUrl
                    venue.ImageUrl = await UploadImageToBlobAsync(venue.ImageFile);
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            var hasBookings = await _context.Booking.AnyAsync(b => b.VenueID == id);
            if (hasBookings)
            //ChatGpt was used to assist in the rules of the modelstate.Another script was
            //used as a reference so as to imitate the functionality.
            {
                ModelState.AddModelError("", "Cannot delete venue with existing bookings.");
                return View("Index", await _context.Venue.ToListAsync());
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // ============================
        // Upload image to Azurite Blob Storage
        // ============================
        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            // Read Azurite connection string from appsettings
            var connectionString = _configuration["BlobStorage:ConnectionString"];

            // Container name must be lowercase
            var containerName = _configuration["BlobStorage:ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);



            // Create the container if it does not already exist
            // PublicAccessType.Blob allows images to be opened directly in the browser
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Ensure the container allows public read access for blobs
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);



            // Generate unique file name to avoid duplicates
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            // Return the blob URL so it can be saved in the database
            return blobClient.Uri.ToString();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueID == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        
    }
}
