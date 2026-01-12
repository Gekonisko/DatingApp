describe('Profile management', () => {
  const user = { password: 'Pass1234' }

  beforeEach(() => {
    // use a unique email per run to avoid conflicts with previous test data
    user.email = `e2e+${Date.now()}@test.local`
    // ensure user exists and is logged in via API (faster and more stable)
    cy.apiRegister({ email: user.email, displayName: 'E2E', password: user.password, gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
    cy.apiLogin(user.email, user.password)
    cy.waitForAppReady()
  })

  it('Edits profile data and shows success', () => {
    // open user dropdown and go to edit profile
    // already logged in via API in beforeEach
    cy.get('img[alt="user avatar"]', { timeout: 20000 }).should('be.visible').click()
    cy.get('a').contains('Edit profile', { matchCase: false }).should('be.visible').click()

    // ensure we're on the member's profile tab and enable edit mode
    cy.contains('a', 'Profile', { timeout: 10000 }).should('be.visible').click()
    cy.contains('button', 'Edit', { timeout: 10000 }).should('be.visible').click()

    // change display name
    cy.get('input[placeholder="Display name"]', { timeout: 20000 }).should('be.visible').as('display')
    cy.get('@display').clear()
    cy.get('@display').type('UpdatedName')
    cy.get('button').contains('Submit').should('not.be.disabled').click()

    // expect toast success
    cy.contains('Profile updated successfully', { timeout: 10000 }).should('exist')
  })
})
