using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Helper
{
    public class OpenAiModelExtend : Model
    {
        /// <summary>
        /// Same capabilities as the standard gpt-3.5-turbo model but with 4 times the context.
        /// </summary>
        public static Model ChatGPTTurbo16K => new Model("gpt-3.5-turbo-16k") { OwnedBy = "openai" };

        /// <summary>
        /// Snapshot of gpt-3.5-turbo-16k from June 13th 2023. Unlike gpt-3.5-turbo-16k, this model will not receive updates, and will be deprecated 3 months after a new version is released.
        /// </summary>
        public static Model ChatGPTTurbo16K0613 => new Model("gpt-3.5-turbo-16k-0613") { OwnedBy = "openai" };

        /// <summary>
        /// Snapshot of gpt-3.5-turbo-16k from June 13th 2023. Unlike gpt-3.5-turbo-16k, this model will not receive updates, and will be deprecated 3 months after a new version is released.
        /// </summary>
        public static Model Gpt4 => new Model("gpt-4") { OwnedBy = "openai" };

        /// <summary>
        /// Snapshot of gpt-3.5-turbo-16k from June 13th 2023. Unlike gpt-3.5-turbo-16k, this model will not receive updates, and will be deprecated 3 months after a new version is released.
        /// </summary>
        public static Model Gpt40613 => new Model("gpt-4-0613") { OwnedBy = "openai" };


        /// <summary>
        /// Snapshot of gpt-3.5-turbo-16k from June 13th 2023. Unlike gpt-3.5-turbo-16k, this model will not receive updates, and will be deprecated 3 months after a new version is released.
        /// </summary>
        public static Model Gpt41106 => new Model("gpt-4-1106-preview") { OwnedBy = "openai" };
    }
}
