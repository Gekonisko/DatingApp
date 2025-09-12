import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { authGuard } from '../core/guards/auth-guard-guard';
import { MemberList } from '../features/members/member-list/member-list';
import { MemberDetailed } from '../features/members/member-detailed/member-detailed';
import { Lists } from '../features/lists/lists';
import { Messages } from '../features/messages/messages';
import { TestErrors } from '../features/test-errors/test-errors';
import { ServerError } from '../shared/server-error/server-error';
import { NotFound } from '../shared/not-found/not-found';

export const routes: Routes = [
    { path: '', component: Home },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            { path: 'members', component: MemberList },
            { path: 'members/:id', component: MemberDetailed },
            { path: 'lists', component: Lists },
            { path: 'messages', component: Messages },
        ]
    },
    { path: 'errors', component: TestErrors },
    { path: 'server-error', component: ServerError },
    { path: '**', component: NotFound },
];