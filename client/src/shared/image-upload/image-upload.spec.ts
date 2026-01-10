import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ImageUpload } from './image-upload';

describe('ImageUpload', () => {
  let component: ImageUpload;
  let fixture: ComponentFixture<ImageUpload>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImageUpload], // standalone component
      providers: [provideZonelessChangeDetection()]
    }).compileComponents();

    fixture = TestBed.createComponent(ImageUpload);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should set isDragging to true on drag over', () => {
    const event = new DragEvent('dragover');
    spyOn(event, 'preventDefault');
    spyOn(event, 'stopPropagation');

    component.onDragOver(event);

    expect(event.preventDefault).toHaveBeenCalled();
    expect(event.stopPropagation).toHaveBeenCalled();
    expect((component as any).isDragging).toBeTrue();
  });

  it('should set isDragging to false on drag leave', () => {
    const event = new DragEvent('dragleave');
    spyOn(event, 'preventDefault');
    spyOn(event, 'stopPropagation');

    component.onDragLeave(event);

    expect(event.preventDefault).toHaveBeenCalled();
    expect(event.stopPropagation).toHaveBeenCalled();
    expect((component as any).isDragging).toBeFalse();
  });

  it('should preview image and store file on drop', () => {
    const file = new File(['test'], 'test.png', { type: 'image/png' });

    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);

    const event = new DragEvent('drop', {
      dataTransfer,
    });

    spyOn(event, 'preventDefault');
    spyOn(event, 'stopPropagation');

    component.onDrop(event);

    expect(event.preventDefault).toHaveBeenCalled();
    expect(event.stopPropagation).toHaveBeenCalled();
    expect((component as any).fileToUpload).toBe(file);
  });

  it('should emit file on upload', () => {
    const file = new File(['test'], 'test.png', { type: 'image/png' });
    (component as any).fileToUpload = file;

    spyOn(component.uploadFile, 'emit');

    component.onUploadFile();

    expect(component.uploadFile.emit).toHaveBeenCalledWith(file);
  });

  it('should not emit file if no file is selected', () => {
    spyOn(component.uploadFile, 'emit');

    component.onUploadFile();

    expect(component.uploadFile.emit).not.toHaveBeenCalled();
  });
});
