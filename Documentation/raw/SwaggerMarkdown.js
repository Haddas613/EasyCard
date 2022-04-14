const widdershins = require('widdershins');
let options = {}; // defaults shown
options.codeSamples = false;
options.httpsnippet = false;
options.language_tabs = [];
//options.language_clients = [];
//options.loadedFrom = sourceUrl; // only needed if input document is relative
//options.user_templates = './user_templates';
//options.templateCallback = function(templateName,stage,data) { return data };
options.theme = 'darkula';
options.search = true;
options.sample = true; // set false by --raw
options.discovery = false;
options.includes = [];
options.shallowSchemas = false;
options.tocSummary = true;
options.headings = 2;
options.yaml = false;
options.expandBody = false;
options.omitBody = true;
options.shallowSchemas = true;
options.useBodyName = true;
options.verbose = true;

//options.resolve = false;
//options.source = sourceUrl; // if resolve is true, must be set to full path or URL of the input document

const fs = require('fs');
const fileData = fs.readFileSync('swagger.json', 'utf8');
const swaggerFile = JSON.parse(fileData);

widdershins.convert(swaggerFile, options)
.then(markdownOutput => {
  // markdownOutput contains the converted markdown
  fs.writeFileSync('myOutput.md', markdownOutput, 'utf8');
})
.catch(err => {
  console.log(err);
});