const myArgs = process.argv.slice(2);
console.log(myArgs);

const widdershins = require('./widdershins');
let options = {}; // defaults shown
options.codeSamples = false;
options.httpsnippet = false;
options.language_tabs =  [];
options.language_clients = [];
options.lang = false;
//options.loadedFrom = sourceUrl; // only needed if input document is relative
//options.user_templates = './user_templates';
//options.templateCallback = function(templateName,stage,data) { return data };
options.theme = 'darkula';
options.search = false;
options.sample = true; // set false by --raw
options.discovery = false;
options.includes = [];
options.tocSummary = true;
options.headings = 2;
options.yaml = false;
options.expandBody = false;
options.omitBody = false;
options.shallowSchemas = false;
options.useBodyName = false;
options.verbose = true;
options.omitHeader = true;

//options.resolve = false;
//options.source = sourceUrl; // if resolve is true, must be set to full path or URL of the input document

const fs = require('fs');
const fileData = fs.readFileSync(myArgs[0], 'utf8');
const swaggerFile = JSON.parse(fileData);

widdershins.convert(swaggerFile, options)
.then(markdownOutput => {
  // markdownOutput contains the converted markdown
  fs.writeFileSync(myArgs[1], markdownOutput, 'utf8');
})
.catch(err => {
  console.log(err);
});