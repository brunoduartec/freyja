import Foundation
import AVFoundation
import AppKit
import CoreVideo

struct Frame: Decodable { let file: String; let time: Double }
let dir = URL(fileURLWithPath: CommandLine.arguments[1])
let output = URL(fileURLWithPath: CommandLine.arguments[2])
let frames = try JSONDecoder().decode([Frame].self, from: Data(contentsOf: dir.appendingPathComponent("frames.json")))
let writer = try AVAssetWriter(outputURL: output, fileType: .mp4)
let width=960, height=540
let input = AVAssetWriterInput(mediaType: .video, outputSettings: [AVVideoCodecKey:AVVideoCodecType.h264, AVVideoWidthKey:width, AVVideoHeightKey:height, AVVideoCompressionPropertiesKey:[AVVideoAverageBitRateKey:3000000]])
let adaptor=AVAssetWriterInputPixelBufferAdaptor(assetWriterInput: input, sourcePixelBufferAttributes:[kCVPixelBufferPixelFormatTypeKey as String:kCVPixelFormatType_32ARGB,kCVPixelBufferWidthKey as String:width,kCVPixelBufferHeightKey as String:height])
writer.add(input)
guard writer.startWriting() else {fatalError("Writer: \(String(describing:writer.error))")}
writer.startSession(atSourceTime:.zero)
for frame in frames {
 guard let image=NSImage(contentsOf:dir.appendingPathComponent("frames/"+frame.file)),let cg=image.cgImage(forProposedRect:nil,context:nil,hints:nil) else {fatalError("Invalid frame")}
 var pixel:CVPixelBuffer?
 CVPixelBufferCreate(kCFAllocatorDefault,width,height,kCVPixelFormatType_32ARGB,[kCVPixelBufferCGImageCompatibilityKey:true,kCVPixelBufferCGBitmapContextCompatibilityKey:true] as CFDictionary,&pixel)
 let buffer=pixel!;CVPixelBufferLockBaseAddress(buffer,[])
 let context=CGContext(data:CVPixelBufferGetBaseAddress(buffer),width:width,height:height,bitsPerComponent:8,bytesPerRow:CVPixelBufferGetBytesPerRow(buffer),space:CGColorSpaceCreateDeviceRGB(),bitmapInfo:CGImageAlphaInfo.noneSkipFirst.rawValue)!
 context.interpolationQuality = .none;context.draw(cg,in:CGRect(x:0,y:0,width:width,height:height));CVPixelBufferUnlockBaseAddress(buffer,[])
 while !input.isReadyForMoreMediaData {Thread.sleep(forTimeInterval:0.01)}
 guard adaptor.append(buffer,withPresentationTime:CMTime(seconds:frame.time,preferredTimescale:60000)) else {fatalError("Append: \(String(describing:writer.error))")}
}
input.markAsFinished();let done=DispatchSemaphore(value:0);writer.finishWriting {done.signal()};done.wait()
guard writer.status == .completed else {fatalError("Finish: \(String(describing:writer.error))")}
print("frames=\(frames.count) seconds=\(frames.last!.time) output=\(output.path)")
