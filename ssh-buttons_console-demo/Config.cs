using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ssh_buttons_console_demo
{
    public partial class Config
    {
        public string[] LoadConfig()
        {
            
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

                string json = File.ReadAllText(filePath);

                JObject jsonObject = JObject.Parse(json);

                string[] loadedCommands = jsonObject.Properties().Select(p => p.Value.ToString()).ToArray();

                return loadedCommands;
            }
            catch (FileNotFoundException ex)
            {
                string[] loadedCommands = new string[] { "error", "error", "Error: Configuration file was not loaded", "error", ex.Message, "error" };

                return loadedCommands;
            }
            catch (JsonException ex)
            {
                string[] loadedCommands = new string[] { "error", "error", "Error: Configuration file was not loaded", "error", ex.Message, "error" };

                return loadedCommands;
            }
            catch (Exception ex)
            {
                string[] loadedCommands = new string[] { "error", "error", "Error: Configuration file was not loaded", "error", ex.Message, "error" };

                return loadedCommands;
            }
        }
    }
}
