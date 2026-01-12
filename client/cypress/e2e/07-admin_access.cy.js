describe('Admin access', () => {
  it('Admin user can access admin panel', () => {
    const admin = { email: 'admin@test.local', password: 'Pass1234' }
    cy.visit('/')
    cy.ensureLoggedIn(admin.email, admin.password)
    cy.visit('/admin')
    cy.url().should('include', '/admin')
  })

  it('Non-admin user is blocked from admin panel', () => {
    const user = { email: 'e2enonadmin@test.local', password: 'Pass1234' }
    // ensure non-admin exists via API
    cy.apiRegister({ ...user, displayName: 'E2ENoAdmin', gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
    cy.visit('/')
    cy.get('input[placeholder="Email"]').type(user.email)
    cy.get('input[placeholder="Password"]').type(user.password)
    cy.get('button').contains('Login').click()
    cy.visit('/admin')
    // expect redirect or access denied - adjust depending on app behavior
    cy.url().should('not.include', '/admin')
  })
})
