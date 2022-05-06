// Initialize the application

const initEcwid = function (app_id) {

	EcwidApp.init({
		app_id: app_id, //"custom-app-70394425-1", // use your application namespace
		autoloadedflag: true,
		autoheight: true,
		installed: "yes"
	});
	const self = this;
	self.storeData = EcwidApp.getPayload();

	self.storeId = self.storeData.store_id;
	self.accessToken = self.storeData.access_token;
	self.language = self.storeData.lang;
	self.viewMode = self.storeData.view_mode;

	if (self.storeData.public_token !== undefined) {
		self.publicToken = self.storeData.public_token;
	}

	if (self.storeData.app_state !== undefined) {
		self.appState = self.storeData.app_state;
	}

	// Function to go to edit product label page

	self.showEditPage = function(elementClass) {
		document.querySelector('.main').style.display = 'none';
		document.querySelector(elementClass).style.display = 'block';
		document.querySelector('.content-control-menu-nav').style.display = 'flex';
	}


	// Function to retutn back to main app page after editing product label page

	self.goBack = function() {
		document.querySelector('.content-control-menu-nav').style.display = 'none';
		document.querySelector('.main').style.display = 'block';

		// hide all separate pages for editing
		for (i = 0; i < document.querySelectorAll('.separate-editing-page').length; i++) {
			document.querySelectorAll('.separate-editing-page')[i].style.display = 'none';
		}
	}


	// Reads values from HTML page and sends them to application config
	// To fill values successfully, the input, select or textarea elements on a page must have 'data-name' and 'data-visibility' attributes set. See appProto.html for examples

	self.readValuesFromPage = function() {

		var applicationConfig = {
			public: {},
			private: {}
		}

		var allInputs = document.querySelectorAll('input, select, textarea');

		for (i = 0; i < allInputs.length; i++) {
			var fieldVisibility = allInputs[i].dataset.visibility;

			if (fieldVisibility !== undefined) {
				if (allInputs[i].tagName == "INPUT") {

					if (allInputs[i].type == 'checkbox' || allInputs[i].type == 'radio') {
						applicationConfig[fieldVisibility][allInputs[i].dataset.name] = String(allInputs[i].checked);
					}
					if (allInputs[i].type == 'text' || allInputs[i].type == 'number' || allInputs[i].type == 'date') {
						applicationConfig[fieldVisibility][allInputs[i].dataset.name] = allInputs[i].value;
					}
				}
				if (allInputs[i].tagName == "SELECT" || allInputs[i].tagName == "TEXTAREA") {
					applicationConfig[fieldVisibility][allInputs[i].dataset.name] = allInputs[i].value;
				}
			}
		}

		applicationConfig.public = JSON.stringify(applicationConfig.public);

		return applicationConfig;
	}

	// Reads values from provided config and sets them for inputs on the page. 
	// To fill values successfully, the input, select or textarea elements must have 'data-name' and 'data-visibility' attributes set. See appProto.html for examples

	self.setValuesForPage = function(applicationConfig) {

		var applicationConfigTemp = {
			public: {},
			private: {}
		};

		// for cases when we get existing users' data

		if (applicationConfig.constructor === Array) {
			for (i = 0; i < applicationConfig.length; i++) {
				if (applicationConfig[i].key !== 'public') {
					applicationConfigTemp.private[applicationConfig[i].key] = applicationConfig[i].value;
				} else {
					applicationConfigTemp[applicationConfig[i].key] = applicationConfig[i].value;
				}
			}
			self.applicationConfig = applicationConfigTemp;
		}

		self.applicationConfig.public = JSON.parse(self.applicationConfig.public);
		var allInputs = document.querySelectorAll('input, select, textarea');

		// Set values from config for input, select, textarea elements

		for (i = 0; i < allInputs.length; i++) {
			var fieldVisibility = allInputs[i].dataset.visibility;

			if (fieldVisibility !== undefined && applicationConfig[fieldVisibility][allInputs[i].dataset.name] !== undefined) {
				if (allInputs[i].tagName == "INPUT") {

					if (allInputs[i].type == 'checkbox' || allInputs[i].type == 'radio') {
						allInputs[i].checked = (applicationConfig[fieldVisibility][allInputs[i].dataset.name] == "true");
						checkFieldChange(allInputs[i]);
					}
					if (allInputs[i].type == 'text' || allInputs[i].type == 'number' || allInputs[i].type == 'date') {
						allInputs[i].value = applicationConfig[fieldVisibility][allInputs[i].dataset.name];
						checkFieldChange(allInputs[i]);
					}
				}
				if (allInputs[i].tagName == "SELECT" || allInputs[i].tagName == "TEXTAREA") {
					allInputs[i].value = applicationConfig[fieldVisibility][allInputs[i].dataset.name];
					checkFieldChange(allInputs[i]);
				}
			}
		}
	}

	// Default settings for new accounts

	self.initialConfig = {
		private: {
			apiKey: "example-api-key",
			installed: "yes"
		}
	};

	self.initialConfig.public = JSON.stringify(self.initialConfig.public);

	// Executes when we have a new user install the app. It creates and sets the default data using Ecwid JS SDK and Application storage

	self.createUserData = function() {

		// Saves data for application storage 
		EcwidApp.setAppStorage(initialConfig.private, function (value) {
			console.log('Initial private user preferences saved!');
		});

		// Saves data for public app config
		EcwidApp.setAppPublicConfig(initialConfig.public, function (value) {
			console.log('Initial public user preferences saved!');
		});

		// Function to prepopulate values of select, input and textarea elements based on default settings for new accounts
		self.setValuesForPage(self.initialConfig);
	}


	// Executes if we have a user who logs in to the app not the first time. We load their preferences from Application storage with Ecwid JS SDK and display them in the app interface

	self.getUserData = function() {

		// Retrieve all keys and values from application storage, including public app config. Set the values for select, input and textarea elements on a page in a callback

		EcwidApp.getAppStorage(function (allValues) {
			self.setValuesForPage(allValues);
		});

	}

	// Executes when we need to save data. Gets all elements' values and saves them to Application storage and public app config via Ecwid JS SDK

	self.saveUserData = function() {

		var saveData = self.readValuesFromPage();

		EcwidApp.setAppStorage(saveData.private, function (savedData) {
			console.log('Private preferences saved!');
		});

		EcwidApp.setAppPublicConfig(saveData.public, function (savedData) {
			console.log('Public preferences saved!');
		})

	}


	// Main app function to determine if the user is new or just logs into the app

	EcwidApp.getAppStorage('installed', function (value) {

		if (value != null) {
			self.getUserData();
		}
		else {
			self.createUserData();
		}
	})


}