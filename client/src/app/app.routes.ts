import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { authGuard } from '../core/guards/auth-guard';
import { MemberList } from '../features/members/member-list/member-list';
import { MemberDetailed } from '../features/members/member-detailed/member-detailed';
import { Lists } from '../features/lists/lists';
import { Messages } from '../features/messages/messages';
import { TestErrors } from '../features/test-errors/test-errors';
import { ServerError } from '../shared/server-error/server-error';
import { NotFound } from '../shared/not-found/not-found';
import { memberResolver } from '../member-resolver';
import { MemberProfile } from '../features/members/member-profile/member-profile';
import { MemberPhotos } from '../features/members/member-photos/member-photos';
import { MemberMessages } from '../features/members/member-messages/member-messages';

export const routes: Routes = [
    { path: '', component: Home },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            { path: 'members', component: MemberList },
            {
                path: 'members/:id',
                resolve: { member: memberResolver },
                runGuardsAndResolvers: 'always',
                component: MemberDetailed,
                children: [
                    { path: '', redirectTo: 'profile', pathMatch: 'full' },
                    { path: 'profile', component: MemberProfile, title: 'Profile' },
                    { path: 'photos', component: MemberPhotos, title: 'Photos' },
                    { path: 'messages', component: MemberMessages, title: 'Messages' },
                ]

            },
        ]
    },
    { path: 'errors', component: TestErrors },
    { path: 'server-error', component: ServerError },
    { path: '**', component: NotFound },
];