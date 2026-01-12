describe('Likes', () => {
  const user = { email: 'admin@test.local', password: 'Pass1234' }

  before(() => {
    cy.apiRegister({ email: user.email, displayName: 'E2E', password: user.password, gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
    cy.apiLogin(user.email, user.password)
    cy.waitForAppReady()
    // navigate via UI to ensure router and components are initialized
    cy.visit('/')
    cy.waitForAppReady()
    cy.get('nav').contains('Matches').click()
  })

  it('Adds and removes like on member card', () => {
    // click first member card like button
    cy.get('.card', { timeout: 10000 }).first().within(() => {
      cy.get('button').first().should('not.be.disabled').click()
    })

    // click again to remove
    cy.get('.card').first().within(() => {
      cy.get('button').first().should('not.be.disabled').click()
    })
  })
})
