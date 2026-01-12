import './commands'

// ignore uncaught exceptions from third-party libs
Cypress.on('uncaught:exception', (err, runnable) => {
  // returning false here prevents Cypress from failing the test
  return false
})
