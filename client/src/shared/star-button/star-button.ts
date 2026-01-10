import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-star-button',
  standalone: true,
  imports: [],
  templateUrl: './star-button.html',
  styleUrls: ['./star-button.css']
})
export class StarButton {
  disabled = input<boolean>();
  selected = input<boolean>();
  isStarred = input<boolean>();
  clickEvent = output<Event>();

  onClick(event: Event) {
    this.clickEvent.emit(event);
  }
}