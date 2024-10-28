using Microsoft.SemanticKernel;

namespace Assistants.API.Core
{
    public class OpenAIClientFacade
    {
        // Dictionary to hold kernels based on their version deployment names
        private Dictionary<string, Kernel> kernels = new Dictionary<string, Kernel>();
        string _kernel3DeploymentName = "";
        string _kernel4DeploymentName = "";

        // Constructor initializing the facade with two kernels
        public OpenAIClientFacade(string kernel3DeploymentName, Kernel kernel3, string kernel4DeploymentName, Kernel kernel4)
        {
            _kernel3DeploymentName = kernel3DeploymentName;
            _kernel4DeploymentName = kernel4DeploymentName;
            kernels[kernel3DeploymentName] = kernel3;
            kernels[kernel4DeploymentName] = kernel4;
        }

        public void RegisterKernel(string deploymentName, Kernel kernel)
        {
            kernels[deploymentName] = kernel;
        }

        // Retrieves the kernel based on the deployment name
        public Kernel GetKernelByDeploymentName(string deploymentName)
        {
            if (kernels.ContainsKey(deploymentName))
            {
                return kernels[deploymentName];
            }
            throw new ArgumentException("Deployment name not recognized.");
        }

        // Public method to retrieve kernel using a boolean flag to determine which version
        public Kernel GetKernel(bool useChatGPT4)
        {
            return useChatGPT4 ? GetKernelByDeploymentName(_kernel4DeploymentName) : GetKernelByDeploymentName(_kernel3DeploymentName);
        }

        // Public method to retrieve the kernel deployment name using a boolean flag
        public string GetKernelDeploymentName(bool useChatGPT4)
        {
            return useChatGPT4 ? _kernel4DeploymentName : _kernel3DeploymentName;
        }
    }

}
