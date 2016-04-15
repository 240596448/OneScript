#!/bin/sh

SRCPATH=/media
BINPATH=${SRCPATH}/bin/
DEBBUILDROOT=${SRCPATH}/bin/
BUILDERROOT=${SRCPATH}/deb/

VERSION=$(cat ${BINPATH}VERSION)
PAKNAME=onescript-engine
DSTPATH=${DEBBUILDROOT}${PAKNAME}

mkdir $DSTPATH 
mkdir -p $DSTPATH/DEBIAN
mkdir -p $DSTPATH/usr/bin
mkdir -p $DSTPATH/usr/share/oscript/lib
mkdir -p $DSTPATH/usr/share/oscript/bin
mkdir -p $DSTPATH/etc

cp ${BUILDERROOT}settings/dirs $DSTPATH/DEBIAN/
cat ${BUILDERROOT}settings/control | sed -r "s/VERSION/$VERSION/g" > $DSTPATH/DEBIAN/control
cp ${BINPATH}*.exe $DSTPATH/usr/share/oscript/bin
cp ${BINPATH}*.dll $DSTPATH/usr/share/oscript/bin
cp ${BUILDERROOT}oscript $DSTPATH/usr/bin
cp ${BUILDERROOT}oscript-cgi $DSTPATH/usr/bin
cp -r ${SRCPATH}/lib/src/* $DSTPATH/usr/share/oscript/lib
cp ${BINPATH}/oscript.cfg $DSTPATH/etc

# TODO: Убрать это!
cp ${BINPATH}/oscript.cfg $DSTPATH/usr/share/oscript/bin

fakeroot dpkg-deb --build $DSTPATH

rm -rf $DSTPATH
chmod 777 $DSTPATH.deb

