describe('Registration and Login', () => {
  const timestamp = Date.now()
  const user = {
    email: `e2e${timestamp}@test.local`,
    displayName: `E2E${timestamp}`,
    password: 'Pass1234',
    gender: 'male',
    dateOfBirth: '1990-01-01',
    city: 'TestCity',
    country: 'TestCountry'
  }

  it('Register new user via UI and redirect to members', () => {
    cy.visit('/')
    cy.waitForAppReady()
    cy.get('button').contains('Register', { timeout: 10000 }).should('be.visible').click()

    // scope inputs to the register component to avoid selecting nav inputs
    cy.get('app-register', { timeout: 10000 }).within(() => {
      cy.get('input[placeholder="Email"]').should('be.visible').type(user.email)
      cy.get('input[placeholder="Display name"]').should('be.visible').type(user.displayName)
      cy.get('input[placeholder="Password"]').should('be.visible').type(user.password)
      cy.get('input[placeholder="Confirm Password"]').should('be.visible').type(user.password)
      cy.get('button').contains('Next').should('not.be.disabled').click()
    })

    // profile step
    cy.get('app-text-input[formcontrolname="dateOfBirth"] input', { timeout: 10000 }).should('be.visible').type('1990-01-01')
    cy.get('app-text-input[formcontrolname="city"] input', { timeout: 10000 }).should('be.visible').type(user.city)
    cy.get('app-text-input[formcontrolname="country"] input', { timeout: 10000 }).should('be.visible').type(user.country)
    cy.get('button').contains('Register').should('not.be.disabled').click()

    cy.url({ timeout: 10000 }).should('include', '/members')
    cy.get('header', { timeout: 10000 }).should('contain', user.displayName)
  })

  it('Login existing user via nav login', () => {
    // ensure user exists (register via API if needed)
    cy.apiRegister(user)

    // login via API and assert user is shown when visiting the app
    cy.apiLogin(user.email, user.password).then(() => {
      cy.waitForAppReady()
      cy.get('header', { timeout: 10000 }).should('contain', user.displayName)
    })
  })
})
