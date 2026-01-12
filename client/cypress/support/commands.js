// Custom commands for API-backed setup and UI helpers

Cypress.Commands.add('apiRegister', (user) => {
  return cy.request({
    method: 'POST',
    url: 'https://localhost:5001/api/account/register',
    body: user,
    failOnStatusCode: false,
  })
})
Cypress.Commands.add('apiLogin', (email, password) => {
  // Login via API and then open app with user persisted in localStorage
  return cy.request({
    method: 'POST',
    url: 'https://localhost:5001/api/account/login',
    body: { email, password },
    failOnStatusCode: false
  }).then((resp) => {
    return resp
  }).then((resp) => {
    if (resp.status === 200 && resp.body) {
      // visit the frontend using configured baseUrl so Cypress resolves the correct host/port
      return cy.visit('/', {
        onBeforeLoad(win) {
          try { win.localStorage.setItem('user', JSON.stringify(resp.body)) } catch (e) {}
        }
      }).then(() => resp)
    }
    return resp
  })
})

Cypress.Commands.add('waitForAppReady', () => {
  // wait for app initializer splash to be removed and router to be active
  cy.get('#initial-splash', { timeout: 20000 }).should('not.exist')
  cy.get('body', { timeout: 20000 }).should('be.visible')
})
 

Cypress.Commands.add('uiLogin', (email, password) => {
  // login using the UI form in the nav
  cy.get('input[placeholder="Email"]').clear().type(email)
  cy.get('input[placeholder="Password"]').clear().type(password)
  cy.get('form').contains('Login').click()
})

Cypress.Commands.add('ensureLoggedIn', (email, password) => {
  // if already logged in, return; otherwise perform UI login
  cy.get('body').then($body => {
    if ($body.find('img[alt="user avatar"]' ).length > 0) {
      return
    }
    cy.uiLogin(email, password)
  })
})
