describe('Filtering members', () => {
  const user = { email: 'admin@test.local', password: 'Pass1234' }

  before(() => {
    cy.apiRegister({ email: user.email, displayName: 'E2E', password: user.password, gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
    cy.apiLogin(user.email, user.password)
    cy.waitForAppReady()
    // navigate via UI to ensure app router is active
    cy.visit('/')
    cy.waitForAppReady()
    cy.get('nav').contains('Matches').click()
  })

  it('Applies filters and updates list', () => {
    cy.get('button').contains('Select Filters', { matchCase: false, timeout: 10000 }).should('be.visible').click()
    // choose gender female
    cy.get('input[name="gender"][value="female"]', { timeout: 10000 }).check({ force: true })
    cy.get('input[name="minAge"]', { timeout: 10000 }).clear().type('25')
    cy.get('input[name="maxAge"]', { timeout: 10000 }).clear().type('40')
    cy.get('button').contains('Submit').should('not.be.disabled').click()

    // results list should be filtered (basic assertion)
    cy.get('.card', { timeout: 10000 }).should('exist')
  })
})
