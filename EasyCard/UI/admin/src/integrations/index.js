// Globally register all components. Components are registered using their file name.

import Vue from 'vue'

// https://webpack.js.org/guides/dependency-management/#require-context
const requireComponent = require.context(
    // Look for files in the current directory
    '.',
    true,
    /[\w-]+\.vue$/
)

// For each matching file name...
requireComponent.keys().forEach((fileName) => {
    // Get the component config
    const componentConfig = requireComponent(fileName)
    
    let componentName = (componentConfig.default || componentConfig).name;
    if(!componentName){
        let split = fileName.split('/');
        fileName = split[split.length - 1];

        componentName = fileName
        // Remove the file extension from the end
        .replace(/\.\w+$/, '')
        .split(/(?=[A-Z]+[^A-Z]?)/)
        .map(v => v.toLowerCase())
        .join("-")
    }

    // Globally register the component
    Vue.component(componentName, componentConfig.default || componentConfig)
})