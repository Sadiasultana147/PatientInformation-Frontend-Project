using PatientInformation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace PatientInformation.Controllers
{
    public class PatientController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7272/api/");
        }

        public async Task<IActionResult> AddPatient()


        {
            try
            {
                var diseaseResponse = await _httpClient.GetAsync("DiseaseInformation");
                if (!diseaseResponse.IsSuccessStatusCode)
                {
                   
                    return StatusCode((int)diseaseResponse.StatusCode, $"Failed to retrieve disease data from API: {diseaseResponse.ReasonPhrase}");
                }

                var diseaseJsonString = await diseaseResponse.Content.ReadAsStringAsync();
                var diseaseOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                };
                var diseases = JsonSerializer.Deserialize<List<DiseaseInformationModel>>(diseaseJsonString, diseaseOptions);

                var diseaseSelectList = diseases.Select(d => new SelectListItem
                {
                    Text = d.DiseaseName,
                    Value = d.DiseaseID.ToString()
                }).ToList();

                diseaseSelectList.Insert(0, new SelectListItem
                {
                    Text = "Select a Disease",
                    Value = "0"
                });

                var ncdResponse = await _httpClient.GetAsync("NCD/NCD");

                if (!ncdResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)ncdResponse.StatusCode, $"Failed to retrieve NCD data from API: {ncdResponse.ReasonPhrase}");
                }
                var ncdJsonString = await ncdResponse.Content.ReadAsStringAsync();
                var ncdOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                };
                var ncds = JsonSerializer.Deserialize<List<NCDViewModel>>(ncdJsonString, ncdOptions);
                
                var ncdSelectList = ncds.Select(n => new SelectListItem
                {
                    Text = n.NCDName,
                    Value = n.NCDID.ToString()
                }).ToList();
                
                var allergiesResponse = await _httpClient.GetAsync("Allergies");

                if (!allergiesResponse.IsSuccessStatusCode)
                {
                    
                    return StatusCode((int)ncdResponse.StatusCode, $"Failed to retrieve allergies data from API: {ncdResponse.ReasonPhrase}");
                }

                var allergiesJsonString = await allergiesResponse.Content.ReadAsStringAsync();
                var allergiesOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                };
                var allergies = JsonSerializer.Deserialize<List<AllergiesModel>>(allergiesJsonString, allergiesOptions);

                var allergiesSelectList = allergies.Select(n => new SelectListItem
                {
                    Text = n.AllergiesName,
                    Value = n.AllergiesID.ToString()
                }).ToList();

                var model = new PatientViewModel
                {
                    DiseaseList = diseaseSelectList,
                    NCDList = ncdSelectList,
                    AllergiesList = allergiesSelectList
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddPatient(PatientViewModel model, string SelectedNCDs = "", string SelectedAllergies = "")
        {
            try
            {
                model.NCDID = SelectedNCDs ;
                model.AllergiesID = SelectedAllergies ;
                var json = JsonSerializer.Serialize(model);
                var response = await _httpClient.PostAsync("PatientInformation", new StringContent(json, System.Text.Encoding.Default, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("PatientList", "Patient");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Failed to add patient: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs during the request, handle the error accordingly
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> PatientList()
        {
            try
            {
                var response = await GetPatientData();

                if (response is ViewResult viewResult && viewResult.Model is IEnumerable<PatientViewModel> patients)
                {
                    
                    return viewResult;
                }
                else
                {
                    
                    return StatusCode(500, "Failed to retrieve patient data from API.");
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, $"Failed to retrieve patient data: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientData()
        {
            try
            {              
                var response = await _httpClient.GetAsync("PatientInformation");
                if (response.IsSuccessStatusCode)
                {                  
                    var responseData = await response.Content.ReadAsStringAsync();
                    var jsonObjects = JsonSerializer.Deserialize<List<dynamic>>(responseData);
                    var patients = new List<PatientViewModel>();
                    foreach (var jsonObject in jsonObjects)
                    {
                        var patient = new PatientViewModel
                        {
                            PatientID = jsonObject.GetProperty("patientID").GetInt32(),

                            PatientName = jsonObject.GetProperty("patientName").GetString(),
                            DiseaseID = jsonObject.GetProperty("diseaseID").GetInt32(),
                            AllergiesID = jsonObject.GetProperty("allergiesID").GetString(),
                            Epilepsy = jsonObject.GetProperty("epilepsy").GetString(),
                            DiseaseName = jsonObject.GetProperty("diseaseName").GetString(),
                            AllergiesName = jsonObject.GetProperty("allergiesName").GetString(),
                            NCDName = jsonObject.GetProperty("ncdName").GetString(),
                        };
                      
                        patients.Add(patient);
                    }

                    return View(patients);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Failed to retrieve patient data: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, $"Failed to retrieve patient data: {ex.Message}");
            }
        }

        // GET: Patient/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"PatientInformation/{id}");

            if (response.IsSuccessStatusCode)
            {
                var patientInfo = await response.Content.ReadFromJsonAsync<PatientViewModel>();
                return View(patientInfo);
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
              
                string requestUri = $"PatientInformation/{id}"; 
                HttpResponseMessage response = await _httpClient.DeleteAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    return NoContent(); 
                }
                else
                {
                    return StatusCode((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
    }







