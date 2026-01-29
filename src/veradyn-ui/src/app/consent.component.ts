import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-consent',
    standalone: true,
    templateUrl: './consent.component.html',
    styleUrls: ['./consent.component.css']
})
export class ConsentComponent implements OnInit {
    scopes: string[] = [];

    constructor() { }

    ngOnInit(): void {
        // In real app, fetch client details and requested scopes from API based on txnId/flowId
        this.scopes = ['openid', 'profile'];
    }

    allow() {
        // Post to API to grant consent
    }

    deny() {
        // Post to API to deny
    }
}
