import Cocoa
import CoreGraphics

let width: Int = 600
let height: Int = 400
let scale: CGFloat = 2.0 // @2x Retina

let pixelWidth = Int(CGFloat(width) * scale)
let pixelHeight = Int(CGFloat(height) * scale)

guard let rep = NSBitmapImageRep(
    bitmapDataPlanes: nil,
    pixelsWide: pixelWidth,
    pixelsHigh: pixelHeight,
    bitsPerSample: 8,
    samplesPerPixel: 4,
    hasAlpha: true,
    isPlanar: false,
    colorSpaceName: .deviceRGB,
    bytesPerRow: pixelWidth * 4,
    bitsPerPixel: 32
) else {
    fatalError("Failed to allocate bitmap")
}

rep.size = NSSize(width: width, height: height)

NSGraphicsContext.saveGraphicsState()
guard let context = NSGraphicsContext(bitmapImageRep: rep) else {
    fatalError("Failed to create graphics context")
}
NSGraphicsContext.current = context
let cgContext = context.cgContext

// 1. Sleek Background (Modern macOS subtle gradient)
let colorSpace = CGColorSpaceCreateDeviceRGB()
let gradientColors = [
    NSColor(calibratedRed: 0.96, green: 0.97, blue: 0.98, alpha: 1.0).cgColor,
    NSColor(calibratedRed: 0.88, green: 0.90, blue: 0.93, alpha: 1.0).cgColor
] as CFArray

if let gradient = CGGradient(colorsSpace: colorSpace, colors: gradientColors, locations: [0.0, 1.0]) {
    cgContext.drawLinearGradient(
        gradient,
        start: CGPoint(x: 0, y: CGFloat(height)), // Top
        end: CGPoint(x: 0, y: 0),                 // Bottom
        options: []
    )
}

// 2. Subtle top border accent line
let topBarColor = NSColor(calibratedRed: 0.80, green: 0.83, blue: 0.87, alpha: 0.6)
topBarColor.setFill()
NSRect(x: 0, y: CGFloat(height) - 1, width: CGFloat(width), height: 1).fill()

// Icon center coordinates (Finder top-left coordinates: left=(160, 190), right=(440, 190))
// In CGContext (from bottom-left): Y = 400 - 190 = 210.
let leftIconCenter = CGPoint(x: 160, y: 210)
let rightIconCenter = CGPoint(x: 440, y: 210)

// 3. Icon placement guide plates (subtle rounded rectangles)
let plateWidth: CGFloat = 116
let plateHeight: CGFloat = 116
let plateCornerRadius: CGFloat = 18

for center in [leftIconCenter, rightIconCenter] {
    let plateRect = NSRect(
        x: center.x - plateWidth / 2,
        y: center.y - plateHeight / 2 + 10,
        width: plateWidth,
        height: plateHeight
    )
    let platePath = NSBezierPath(roundedRect: plateRect, xRadius: plateCornerRadius, yRadius: plateCornerRadius)
    
    // Background plate fill
    NSColor(calibratedWhite: 1.0, alpha: 0.70).setFill()
    platePath.fill()
    
    // Plate subtle border
    NSColor(calibratedRed: 0.75, green: 0.78, blue: 0.84, alpha: 0.8).setStroke()
    platePath.lineWidth = 1.0
    platePath.stroke()
}

// 4. Modern Connecting Arrow (between x=240 and x=360, at y=220)
let arrowStartX: CGFloat = 245
let arrowEndX: CGFloat = 355
let arrowY: CGFloat = 220
let arrowHeadSize: CGFloat = 10

let arrowPath = NSBezierPath()
arrowPath.move(to: NSPoint(x: arrowStartX, y: arrowY))
arrowPath.line(to: NSPoint(x: arrowEndX, y: arrowY))
arrowPath.lineWidth = 3.0
NSColor(calibratedRed: 0.45, green: 0.50, blue: 0.60, alpha: 0.75).setStroke()
arrowPath.stroke()

let headPath = NSBezierPath()
headPath.move(to: NSPoint(x: arrowEndX + 3, y: arrowY))
headPath.line(to: NSPoint(x: arrowEndX - arrowHeadSize, y: arrowY + arrowHeadSize * 0.75))
headPath.line(to: NSPoint(x: arrowEndX - arrowHeadSize, y: arrowY - arrowHeadSize * 0.75))
headPath.close()
NSColor(calibratedRed: 0.45, green: 0.50, blue: 0.60, alpha: 0.9).setFill()
headPath.fill()

// 5. Title & Instructions Typography
let titleParagraphStyle = NSMutableParagraphStyle()
titleParagraphStyle.alignment = .center

let titleFont = NSFont.systemFont(ofSize: 21, weight: .bold)
let titleAttrs: [NSAttributedString.Key: Any] = [
    .font: titleFont,
    .foregroundColor: NSColor(calibratedRed: 0.12, green: 0.14, blue: 0.18, alpha: 1.0),
    .paragraphStyle: titleParagraphStyle
]
let titleString = "Falcon Programmer"
titleString.draw(in: NSRect(x: 20, y: 340, width: CGFloat(width) - 40, height: 32), withAttributes: titleAttrs)

// Subtitle
let subtitleFont = NSFont.systemFont(ofSize: 13, weight: .medium)
let subtitleAttrs: [NSAttributedString.Key: Any] = [
    .font: subtitleFont,
    .foregroundColor: NSColor(calibratedRed: 0.38, green: 0.42, blue: 0.48, alpha: 1.0),
    .paragraphStyle: titleParagraphStyle
]
let subtitleString = "Drag the app to Applications to install"
subtitleString.draw(in: NSRect(x: 20, y: 318, width: CGFloat(width) - 40, height: 22), withAttributes: subtitleAttrs)

// Text below arrow
let dragHelpFont = NSFont.systemFont(ofSize: 11, weight: .semibold)
let dragHelpAttrs: [NSAttributedString.Key: Any] = [
    .font: dragHelpFont,
    .foregroundColor: NSColor(calibratedRed: 0.45, green: 0.50, blue: 0.60, alpha: 0.85),
    .paragraphStyle: titleParagraphStyle
]
let dragHelpString = "INSTALL"
dragHelpString.draw(in: NSRect(x: 230, y: arrowY - 22, width: 140, height: 16), withAttributes: dragHelpAttrs)

NSGraphicsContext.restoreGraphicsState()

guard let pngData = rep.representation(using: .png, properties: [:]) else {
    fatalError("Failed to convert image to PNG")
}

let outputPath = CommandLine.arguments.count > 1 ? CommandLine.arguments[1] : "dmg_background.png"
try! pngData.write(to: URL(fileURLWithPath: outputPath))
print("Successfully generated DMG background image at: \(outputPath)")
