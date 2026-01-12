const { defineConfig } = require('cypress')

module.exports = defineConfig({
  e2e: {
    // frontend dev server runs on HTTPS in this project
    baseUrl: 'https://localhost:4200',
    specPattern: 'cypress/e2e/**/*.cy.js',
    supportFile: 'cypress/support/e2e.js',
    chromeWebSecurity: false,
    viewportWidth: 1280,
    viewportHeight: 720,
    setupNodeEvents(on, config) {
      return config
    }
  }
})
