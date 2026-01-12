describe.skip('Messages (partial)', () => {
  const user = { email: 'admin@test.local', password: 'Pass1234' }

  before(() => {
    cy.apiRegister({ email: user.email, displayName: 'E2E', password: user.password, gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
    cy.apiLogin(user.email, user.password)
    cy.waitForAppReady()
    // navigate via UI to open a member detail and the messages tab (messages component expects a member id)
    cy.visit('/')
    cy.waitForAppReady()
      cy.get('nav').contains('Messages').click()
      // open the first conversation row to load the message thread
      cy.get('table tbody tr', { timeout: 20000 }).first().click()
      cy.waitForAppReady()
  })

  it('Sends a message and it appears in thread (sender view)', () => {
    cy.get('input[placeholder="Enter your message"]', { timeout: 10000 }).should('be.visible').type('Hello from Cypress')
    cy.get('button').contains('Send').should('not.be.disabled').click()

    cy.contains('Hello from Cypress', { timeout: 10000 }).should('exist')
  })

  it.skip('Real-time: recipient receives message without reload (manual/advanced)', () => {
    // This test requires two browser contexts and a running SignalR connection for both users.
    // To verify real-time reception open two browsers and run the steps manually:
    // 1. Login as user B and open conversation
    // 2. Login as user A and send message
    // 3. Observe that user B receives the message without reload
  })
})
