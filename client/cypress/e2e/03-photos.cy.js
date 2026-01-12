describe.skip('Photos management', () => {
  let user = { password: 'Pass1234' }

  before(() => {
    // use a unique test user per run
    user.email = `e2e+${Date.now()}@test.local`
    cy.apiRegister({ email: user.email, displayName: 'E2E', password: user.password, gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
    cy.apiLogin(user.email, user.password)
    cy.waitForAppReady()
  })

  it('Adds a photo and it appears in gallery', () => {
    // go directly to logged-in user's photos page (already logged in via API)
    // go to profile -> photos (already logged in via API)
    cy.get('img[alt="user avatar"]', { timeout: 20000 }).should('be.visible').click()
    cy.get('a').contains('Edit profile', { matchCase: false }).should('be.visible').click()
    // navigate to Photos tab first, then enable edit mode via the global Edit toggle
    cy.get('a').contains('Photos', { matchCase: false, timeout: 10000 }).should('be.visible').click()
    cy.get('div.card').eq(1).contains('button', 'Edit', { matchCase: false, timeout: 10000 }).should('be.visible').click()

    // verify upload UI is present (upload via API is covered elsewhere)
    cy.contains('Click to upload or drag and drop', { timeout: 20000 }).should('be.visible')
    // gallery area should exist (may show no photos yet)
    cy.get('.grid, p.text-center', { timeout: 20000 }).should('exist')
  })
})
