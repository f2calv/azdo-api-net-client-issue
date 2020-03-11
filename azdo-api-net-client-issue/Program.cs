using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
namespace AzDOAPI
{
    class Program
    {
        static string organisation = "myorganisation";//todo:enter accordingly!
        static string projectName = "myproject";//todo:enter accordingly!
        static string PAT = "";//todo:enter accordingly!

        static ProjectHttpClient _projectClient;
        static ReleaseHttpClient _releaseClient;
        static BuildHttpClient _buildClient;

        static async Task Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(PAT))
            {
                Console.WriteLine("Hit any key to exit and enter your Azure DevOps Organisation name, Project name and PAT (Personal Access Token)...");
                Console.ReadKey();
                return;
            }

            var credentials = new VssBasicCredential(string.Empty, PAT);
            var connection = new VssConnection(new Uri($"https://dev.azure.com/{organisation}"), credentials);


            _projectClient = await connection.GetClientAsync<ProjectHttpClient>();
            _releaseClient = await connection.GetClientAsync<ReleaseHttpClient>();
            _buildClient = await connection.GetClientAsync<BuildHttpClient>();

            //get the project
            var project = await _projectClient.GetProject(projectName);
            if (project == null) throw new Exception($"project '{projectName}' not found?");

            //retrieve tasks in release defintion
            var releaseDefinitions = await _releaseClient.GetReleaseDefinitionsAsync(project.Id);
            foreach (var releaseDef in releaseDefinitions)
            {
                var releaseDefinition = await _releaseClient.GetReleaseDefinitionAsync(project.Id, releaseDef.Id);
                if (releaseDefinition == null) throw new Exception($"releaseDefinition '{releaseDef.Id}' not returned?");
                Console.WriteLine($"Release Definition={releaseDefinition.Name}, id={releaseDefinition.Id}");
                if (releaseDefinition.Environments != null && releaseDefinition.Environments.Count > 0)
                {
                    foreach (var environment in releaseDefinition.Environments)
                        foreach (var phase in environment.DeployPhases)
                            foreach (var workflowTask in phase.WorkflowTasks)
                                Console.WriteLine($"taskname={workflowTask.Name}, id={workflowTask.TaskId}");
                    break;//exit loop early
                }
            }

            //now attempt to loop through tasks in build definition;

            //attempt 1 - uncomment below and it will not compile....
            {
                //var buildDefinitions = await _buildClient.GetFullDefinitionsAsync(project.Id);//using GetFullDefinitionsAsync
                //foreach (var buildDefinition in buildDefinitions)
                //{
                //    if (buildDefinition.Process != null && buildDefinition.Process.Phases != null)
                //    {
                //        foreach (var phase in buildDefinition.Process.Phases)
                //            foreach (var step in phase.Steps)
                //                Console.WriteLine($"taskname={step.DisplayName}");
                //        break;//lets exit the loop early
                //    }
                //}
            }

            //attempt 2 - uncomment and will not compile!
            {
                //var buildDefinitions = await _buildClient.GetFullDefinitionsAsync2(project.Id);//using GetFullDefinitionsAsync2
                //foreach (var buildDefinition in buildDefinitions)
                //{
                //    if (buildDefinition.Process != null && buildDefinition.Process.Phases != null)
                //    {
                //        foreach (var phase in buildDefinition.Process.Phases)
                //            foreach (var step in phase.Steps)
                //                Console.WriteLine($"taskname={step.DisplayName}");
                //        break;//lets exit the loop early
                //    }
                //}
            }

            //attempt 3 - uncomment and will not compile!
            {
                //var buildDefinitions = await _buildClient.GetDefinitionsAsync(project.Id);//using GetDefinitionsAsync + GetDefinitionAsync
                //foreach (var buildDef in buildDefinitions)
                //{
                //    var buildDefinition = await _buildClient.GetDefinitionAsync(project.Id, buildDef.Id);
                //    if (buildDefinition.Process != null && buildDefinition.Process.Phases != null)
                //    {
                //        foreach (var phase in buildDefinition.Process.Phases)
                //            foreach (var step in phase.Steps)
                //                Console.WriteLine($"taskname={step.DisplayName}");
                //        break;//lets exit the loop early
                //    }
                //}
            }

            //attempt 4 - working, but only when using dynamic...
            {
                var buildDefinitions = await _buildClient.GetDefinitionsAsync(project.Id);//using GetDefinitionsAsync + GetDefinitionAsync
                foreach (var buildDef in buildDefinitions)
                {
                    var buildDefinition = await _buildClient.GetDefinitionAsync(project.Id, buildDef.Id);
                    if (buildDefinition == null) throw new Exception($"buildDefinition '{buildDef.Id}' not returned?");
                    Console.WriteLine($"Build Definition={buildDefinition.Name}, id={buildDefinition.Id}");
                    var json = JsonConvert.SerializeObject(buildDefinition);
                    //dynamic buildDefinitionB = JArray.Parse(json);
                    dynamic buildDefinitionB = JObject.Parse(json);
                    if (buildDefinitionB.Process != null && buildDefinitionB.Process.Phases != null)
                    {
                        foreach (dynamic phase in buildDefinitionB.Process.Phases)
                            foreach (dynamic step in phase.Steps)
                                Console.WriteLine($"taskname={step.DisplayName}");
                        break;//lets exit the loop early
                    }
                }
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine("What gives?");
            Console.ReadKey();
        }
    }
}